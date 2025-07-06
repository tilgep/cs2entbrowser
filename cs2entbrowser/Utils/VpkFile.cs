using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils;

public class VpkFile
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public List<EntityLump> EntityLumps { get; set; } = new();

    public VpkFile(string name, int id)
    {
        Name = name;
        Id = id;
    }
}
