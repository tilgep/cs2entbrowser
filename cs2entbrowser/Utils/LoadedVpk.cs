using cs2entbrowser.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils;

public class LoadedVpk
{
    public string Title { get; private set; }
    public string Path { get; private set; }
    public List<VpkFileViewModel> VpkFiles { get; private set; } = new();

    public LoadedVpk(string title, string path)
    {
        Title = title; 
        Path = path;
    }
}
