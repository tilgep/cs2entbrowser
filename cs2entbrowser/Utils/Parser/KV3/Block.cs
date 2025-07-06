using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils.Parser.KV3;

public abstract class Block
{
    /// <summary>
    /// Gets the block type.
    /// </summary>
    public abstract BlockType Type { get; }

    /// <summary>
    /// Gets or sets the offset to the data.
    /// </summary>
    public uint Offset { get; set; }

    /// <summary>
    /// Gets or sets the data size.
    /// </summary>
    public uint Size { get; set; }

    /// <summary>
    /// Gets the resource this block belongs to.
    /// </summary>
    //public Resource Resource { get; set; }

    public abstract void Read(BinaryReader reader);


    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        StringWriter w = new StringWriter();
        using var writer = new IndentedTextWriter(w);
        WriteText(writer);

        return w.ToString();
    }

    /// <summary>
    /// Writers the correct object to IndentedTextWriter.
    /// </summary>
    /// <param name="writer">IndentedTextWriter.</param>
    public abstract void WriteText(IndentedTextWriter writer);
}

