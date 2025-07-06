using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils.Parser.KV3;

public class ResourceData : Block
{
    public override BlockType Type => BlockType.DATA;

    public override void Read(BinaryReader reader)
    {
        // TODO
    }

    public override void WriteText(IndentedTextWriter writer)
    {
        throw new NotImplementedException("WriteText() in ResourceData");
    }
}