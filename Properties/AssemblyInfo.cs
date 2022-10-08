using System.Reflection;
using MelonLoader;

[assembly: AssemblyTitle(FP2Rebalance.MichaelGallinago.BuildInfo.Description)]
[assembly: AssemblyDescription(FP2Rebalance.MichaelGallinago.BuildInfo.Description)]
[assembly: AssemblyCompany(FP2Rebalance.MichaelGallinago.BuildInfo.Company)]
[assembly: AssemblyProduct(FP2Rebalance.MichaelGallinago.BuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + FP2Rebalance.MichaelGallinago.BuildInfo.Author)]
[assembly: AssemblyTrademark(FP2Rebalance.MichaelGallinago.BuildInfo.Company)]
[assembly: AssemblyVersion(FP2Rebalance.MichaelGallinago.BuildInfo.Version)]
[assembly: AssemblyFileVersion(FP2Rebalance.MichaelGallinago.BuildInfo.Version)]
[assembly: MelonInfo(typeof(FP2Rebalance.MichaelGallinago.FP2Rebalance), FP2Rebalance.MichaelGallinago.BuildInfo.Name, FP2Rebalance.MichaelGallinago.BuildInfo.Version, FP2Rebalance.MichaelGallinago.BuildInfo.Author, FP2Rebalance.MichaelGallinago.BuildInfo.DownloadLink)]
[assembly: MelonColor()]

// Create and Setup a MelonGame Attribute to mark a Melon as Universal or Compatible with specific Games.
// If no MelonGame Attribute is found or any of the Values for any MelonGame Attribute on the Melon is null or empty it will be assumed the Melon is Universal.
// Values for MelonGame Attribute can be found in the Game's app.info file or printed at the top of every log directly beneath the Unity version.
[assembly: MelonGame(null, null)]