using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs2entbrowser.Utils;

class DirectorySizeCalculator
{
    public static async Task<long> GetDirectorySizeAsync(DirectoryInfo d)
    {
        long size = 0;
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis)
        {
            if (fi.Extension == ".vpk")
                size += fi.Length;
        }
        // Add subdirectory sizes.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            size += await Task.Run(() => GetDirectorySizeAsync(di));
        }
        return size;
    }
}
