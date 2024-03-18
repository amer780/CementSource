using System.Reflection;
using CementTools;
using MelonLoader;

[assembly: AssemblyTitle(CementTools.BuildInfo.Description)]
[assembly: AssemblyDescription(CementTools.BuildInfo.Description)]
[assembly: AssemblyCompany(CementTools.BuildInfo.Company)]
[assembly: AssemblyProduct(CementTools.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + CementTools.BuildInfo.Author)]
[assembly: AssemblyTrademark(CementTools.BuildInfo.Company)]
[assembly: AssemblyVersion(CementTools.BuildInfo.Version)]
[assembly: AssemblyFileVersion(CementTools.BuildInfo.Version)]
[assembly: MelonInfo(typeof(Mod), CementTools.BuildInfo.Name, CementTools.BuildInfo.Version, CementTools.BuildInfo.Author, CementTools.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

[assembly: MelonGame("Boneloaf", "Gang Beasts")]