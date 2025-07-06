using cs2entbrowser.Utils.Parser.KV3.Utils;
using cs2entbrowser.Utils.Parser.KV3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils;

public class Entity
{
    public string EntLumpName { get; set; } = "";
    public KVObject Properties { get; } = new(null);
    // public KVObject Attributes { get; } = new(null);
    public List<KVObject> Connections { get; internal set; }

    public T GetProperty<T>(string name, T defaultValue = default)
    {
        if (typeof(T) == typeof(Vector3))
        {
            throw new InvalidOperationException("Entity.GetProperty<Vector3> has been removed. Use Entity.GetVector3Property.");
        }

        try
        {
            return Properties.GetProperty(name, defaultValue);
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    //public bool TryGetProperty<T>(string name, out T property) => Properties.TryGetProperty(name, out property);

    public T GetPropertyUnchecked<T>(string name, T defaultValue = default)
        => Properties.GetPropertyUnchecked(name, defaultValue);

    public KVValue GetProperty(string name) => Properties.Properties.GetValueOrDefault(name);

    public bool ContainsKey(string name) => Properties.Properties.ContainsKey(name);

    public Vector3 GetVector3Property(string name, Vector3 defaultValue = default)
    {
        if (Properties.Properties.TryGetValue(name, out var value))
        {
            if (value.Value is KVObject kv)
            {
                return kv.ToVector3();
            }

            if (value.Value is string editString)
            {
                return EntityTransformHelper.ParseVector(editString);
            }
        }

        return defaultValue;
    }

    public Vector3 GetColor32Property(string key)
    {
        var defaultColor = new Vector3(255f);
        return GetVector3Property(key, defaultColor) / 255f;
    }
}

