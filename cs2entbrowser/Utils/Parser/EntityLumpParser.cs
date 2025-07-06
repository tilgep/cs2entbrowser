using Avalonia.Platform.Storage;
using cs2entbrowser.ViewModels.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamDatabase.ValvePak;
using System.Diagnostics;
using cs2entbrowser.Services;

namespace cs2entbrowser.Utils.Parser;

static class EntityLumpParser
{
    public static List<VpkFile>? ParseFromVpk(string vpkPath)
    {
        Debug.WriteLine("Parsing vpk...");

        if (!File.Exists(vpkPath))
            return null;

        Debug.WriteLine("Reading with ValvePak...");

        using var package = new Package();
        package.Read(vpkPath);
        package.VerifyHashes();

        int total = 0;
        bool foundALump = false;

        Debug.WriteLine("Searching for .vents_c inside...");
        VpkFile topLevelVpk = new VpkFile(package.FileName.Substring(package.FileName.LastIndexOf('/') + 1), VpkService.Instance.LumpId);
        bool topLevelEnts = package.Entries.TryGetValue("vents_c", out var packageEnts);
        if(!topLevelEnts || packageEnts == null)
        {
            Debug.WriteLine("No top level entity files found.");
            topLevelEnts = false;
        }
        else
        {
            foreach (var mapentry in package.Entries)
            {
                if (mapentry.Key != "vents_c")
                    continue;

                foreach (var entfile in mapentry.Value)
                {
                    EntityLump lump = new EntityLump(VpkService.Instance.LumpId);
                    lump.Read(package.GetMemoryMappedStreamIfPossible(entfile));

                    if (lump.Entities.Count <= 0)
                        continue;

                    VpkService.Instance.LumpId++;

                    if (lump.Name.ToLower() == "default_ents")
                        topLevelVpk.EntityLumps.Insert(0, lump);
                    else
                        topLevelVpk.EntityLumps.Add(lump);
                    Debug.WriteLine("Parsed " + lump.Entities.Count + " entities from lump: " + lump.Name);
                    total += lump.Entities.Count;
                    foundALump = true;

                    // Set entitylump path on every entity
                    foreach(var elump in topLevelVpk.EntityLumps)
                    {
                        foreach(var ent in elump.Entities)
                        {
                            ent.EntLumpName = topLevelVpk.Name + "/" + elump.Name;
                        }
                    }
                }
            }
        }

        Debug.WriteLine("Searching for vpks inside...");

        bool found = package.Entries.TryGetValue("vpk", out var entries);
        if (!found || entries == null)
        {
            Debug.WriteLine("No VPK files found inside");
            if(foundALump)
            {
                List<VpkFile> toplevelfilelist = new();
                toplevelfilelist.Add(topLevelVpk);
                return toplevelfilelist;
            }

            return null;
        }

        Debug.WriteLine("Searching for lumps in vpks...");

        
        List<VpkFile> maps = new();
        foreach (var vpk in entries)
        {
            package.ReadEntry(vpk, out byte[] fileContents);

            using MemoryStream fileStream = new MemoryStream(fileContents);
            using var mapPackage = new Package();
            mapPackage.SetFileName(vpk.GetFullPath());
            mapPackage.Read(fileStream);

            VpkFile vpkFile = new VpkFile(mapPackage.FileName.Substring(mapPackage.FileName.LastIndexOf('/') + 1), VpkService.Instance.LumpId);
            VpkService.Instance.LumpId++;

            foreach (var mapentry in mapPackage.Entries)
            {
                if (mapentry.Key != "vents_c")
                    continue;

                foreach (var entfile in mapentry.Value)
                {
                    EntityLump lump = new EntityLump(VpkService.Instance.LumpId);
                    lump.Read(mapPackage.GetMemoryMappedStreamIfPossible(entfile));

                    if (lump.Entities.Count <= 0)
                        continue;

                    VpkService.Instance.LumpId++;

                    if (lump.Name.ToLower() == "default_ents")
                        vpkFile.EntityLumps.Insert(0, lump);
                    else
                        vpkFile.EntityLumps.Add(lump);
                    Debug.WriteLine("Parsed " + lump.Entities.Count + " entities from lump: " + lump.Name);
                    total += lump.Entities.Count;
                    foundALump = true;

                    // Set entitylump path on every entity
                    foreach (var elump in vpkFile.EntityLumps)
                    {
                        foreach (var ent in elump.Entities)
                        {
                            ent.EntLumpName = vpkFile.Name + "/" + elump.Name;
                        }
                    }
                }
            }

            if (vpkFile.EntityLumps.Count > 0)
                maps.Add(vpkFile);
        }

        Debug.WriteLine("Parsed " + total + " entities in total");

        if (!foundALump)
            return null;

        return maps;
    }
}
