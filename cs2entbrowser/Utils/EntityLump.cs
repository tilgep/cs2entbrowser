using cs2entbrowser.Utils.Parser.KV3;
using cs2entbrowser.Utils.Parser.KV3.Utils;
using cs2entbrowser.ViewModels.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils;

public class EntityLump
{
    public int Id { get; private set; }
    public string Name { get; private set; } = "";
    public string[] ChildLumps { get; private set; } = [];
    public List<Entity> Entities { get; private set; } = new();

    public EntityLump(int id)
    {
        Id = id;
    }
    public void Read(Stream stream)
    {
        BinaryReader Reader = new BinaryReader(stream);

        uint FileSize = Reader.ReadUInt32();
        ushort HeaderVersion = Reader.ReadUInt16();
        ushort Version = Reader.ReadUInt16();

        var blockOffset = Reader.ReadUInt32();
        var blockCount = Reader.ReadUInt32();

        Reader.BaseStream.Position += blockOffset - 8;

        for (int i = 0; i < blockCount; i++)
        {
            var blockType = Encoding.UTF8.GetString(Reader.ReadBytes(4));

            var position = Reader.BaseStream.Position;
            var offset = (uint)position + Reader.ReadUInt32();
            uint size = Reader.ReadUInt32();

            if (size == 0)
                continue;

            if (blockType == nameof(BlockType.DATA))
            {
                var kv3 = new BinaryKV3(BlockType.DATA)
                {
                    Offset = offset,
                    Size = size
                };
                kv3.Read(Reader);
                ParseEntities(kv3.Data);
            }

            Reader.BaseStream.Position = position + 8;
        }
    }

    private void ParseEntities(KVObject data)
    {
        Name = data.GetStringProperty("m_name");
        ChildLumps = data.GetArray<string>("m_childLumps");
        Entities = data.GetArray("m_entityKeyValues").Select(ParseEntityProperties).ToList();
    }

    private static Entity ParseEntityProperties(KVObject entityKv)
    {
        KVObject[] connections = entityKv.GetArray<KVObject>("m_connections");
        Entity entity;

        if (entityKv.ContainsKey("keyValues3Data"))
        {
            entity = ParseEntityPropertiesKV3(entityKv.GetSubCollection("keyValues3Data"));
        }
        else
        {
            entity = ParseEntityProperties(entityKv.GetArray<byte>("m_keyValuesData"));
        }

        if (connections.Length > 0)
        {
            entity.Connections = [.. connections];
        }

        return entity;
    }

    private static Entity ParseEntityPropertiesKV3(KVObject entityKv)
    {
        var entityVersion = entityKv.GetInt32Property("version");

        if (entityVersion != 1)
        {
            throw new UnexpectedMagicException("Unsupported entity data version", entityVersion, nameof(entityVersion));
        }

        var entity = new Entity();

        ReadValues(entity, entityKv.Properties["values"]);
        ReadValues(entity, entityKv.Properties["attributes"]);

        return entity;
    }

    private static void ReadValues(Entity entity, KVValue values)
    {
        if (values.Type != KVType.OBJECT)
        {
            throw new UnexpectedMagicException("Unsupported entity data values type", (int)values.Type, nameof(values.Type));
        }

        var properties = ((KVObject)values.Value).Properties;
        entity.Properties.Properties.EnsureCapacity(entity.Properties.Count + properties.Count);

        foreach (var value in properties)
        {
            // All entity property keys will be stored in lowercase
            var lowercaseKey = value.Key.ToLowerInvariant();

            //var hash = StringToken.Store(lowercaseKey);
            entity.Properties.AddProperty(lowercaseKey, value.Value);
        }
    }

    private static Entity ParseEntityProperties(byte[] bytes)
    {
        using var dataStream = new MemoryStream(bytes);
        using var dataReader = new BinaryReader(dataStream);
        var entityVersion = dataReader.ReadUInt32();

        if (entityVersion != 1)
        {
            throw new UnexpectedMagicException("Unsupported entity data version", entityVersion, nameof(entityVersion));
        }

        var hashedFieldsCount = dataReader.ReadUInt32();
        var stringFieldsCount = dataReader.ReadUInt32();

        var entity = new Entity();

        void ReadTypedValue(uint keyHash, string keyName)
        {
            var type = (EntityFieldType)dataReader.ReadUInt32();

            var (kvType, valueObject) = type switch
            {
                EntityFieldType.Boolean => (KVType.BOOLEAN, (object)dataReader.ReadBoolean()),
                EntityFieldType.Float => (KVType.DOUBLE, (double)dataReader.ReadSingle()),
                EntityFieldType.Float64 => (KVType.DOUBLE, dataReader.ReadDouble()),
                EntityFieldType.Color32 => (KVType.ARRAY, new KVObject("", dataReader.ReadBytes(4).Select(c => new KVValue(KVType.INT64, c)).ToArray())),
                EntityFieldType.Integer => (KVType.INT64, (long)dataReader.ReadInt32()),
                EntityFieldType.UInt => (KVType.UINT64, (ulong)dataReader.ReadUInt32()),
                EntityFieldType.Integer64 => (KVType.UINT64, dataReader.ReadUInt64()), // Is this supposed to be ReadInt64?
                EntityFieldType.Vector or EntityFieldType.QAngle => (KVType.STRING, $"{dataReader.ReadSingle()} {dataReader.ReadSingle()} {dataReader.ReadSingle()}"),
                EntityFieldType.CString => (KVType.STRING, dataReader.ReadNullTermString(Encoding.UTF8)),
                _ => throw new UnexpectedMagicException("Unknown type", (int)type, nameof(type)),
            };

            var entityProperty = new KVValue(kvType, valueObject);

            if (keyName == null)
            {
                keyName = StringToken.GetKnownString(keyHash);
            }
            else
            {
                var calculatedHash = StringToken.Store(keyName);
                if (calculatedHash != keyHash)
                {
                    throw new InvalidDataException(
                        $"Key hash for {keyName} ({keyHash}) found in resource is not the same as the calculated {calculatedHash}."
                    );
                }
            }

            entity.Properties.Properties.Add(keyName, entityProperty);
        }

        for (var i = 0; i < hashedFieldsCount; i++)
        {
            // murmur2 hashed field name (see EntityLumpKeyLookup)
            var keyHash = dataReader.ReadUInt32();

            ReadTypedValue(keyHash, null);
        }

        // TODO: Is this attributes like in KV3 version, should we put them into separate property?
        for (var i = 0; i < stringFieldsCount; i++)
        {
            var keyHash = dataReader.ReadUInt32();
            var keyName = dataReader.ReadNullTermString(Encoding.UTF8);

            ReadTypedValue(keyHash, keyName);
        }

        return entity;
    }
}
