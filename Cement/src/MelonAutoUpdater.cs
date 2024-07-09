using CementGB.Mod.Utilities;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityEngine;

namespace CementGB.Mod;

public static class MelonAutoUpdater
{
    public static async Task UpdateItem(MelonMod melonToUpdate)
    {
        if (Mod.OfflineMode) return;

        var _melonHasValidDownloadLink = Uri.TryCreate(melonToUpdate.Info.DownloadLink, UriKind.Absolute, out var melonReleaseUri)
            && melonReleaseUri.Host.Contains("api.github.com");

        if (!_melonHasValidDownloadLink)
        {
            Melon<Mod>.Logger.Warning($"MelonLoader mod {melonToUpdate} has an invalid or empty DownloadLink field. In order for auto-updating to work, the DownloadLink field must contain an API link to a GitHub release!");
            return;
        }

        Melon<Mod>.Logger.Msg($"Attempting update check for Melon mod {melonToUpdate.Info.Name}. . .");

        var parsedLocalVersion = new Version(melonToUpdate.Info.Version);

        try
        {
            var releaseResponseBody = await Mod.updaterClient.GetStringAsync(melonReleaseUri);
            var releaseResponseObj = (JsonNode.Parse(releaseResponseBody)?.AsObject()) ?? throw new NullReferenceException("Response did not come back, or came back as a non-json value/null.");
            Melon<Mod>.Logger.Msg($"Successfully parsed GitHub API release from mod download link ({melonToUpdate.Info.DownloadLink}). Output: {releaseResponseBody}");

            if (releaseResponseObj.TryGetPropertyValueAs("message", out string? message) && message == "Not Found") throw new Exception("Response came back with 'Not Found' message. This likely means you have the right host name (https://api.github.com), but the repo you requested to doesn't exist or is private.");

            var releaseName = releaseResponseObj["name"]?.AsValue();
            if (releaseName is null || releaseName.GetValue<string>() is null) throw new NullReferenceException($"Release name is null somehow!");

            Melon<Mod>.Logger.Msg($"Found release name: {releaseName}");

            var releaseTag = releaseResponseObj["tag_name"]?.AsValue();
            if (releaseTag is null || releaseTag.GetValue<string>() is null) throw new NullReferenceException($"Release has no tag name, making version checking impossible.");
            if (!Version.TryParse(releaseTag.GetValue<string>(), out var releaseVersion) || releaseVersion is null) throw new NullReferenceException($"Failed to parse tag name of release {releaseName} as type {nameof(Version)}.");

            if (releaseVersion <= parsedLocalVersion)
            {
                Melon<Mod>.Logger.Msg($"Mod {melonToUpdate.Info.Name} is up to date! No higher versioned releases found.");
                return;
            }

            var assetsResponseObj = (releaseResponseObj["assets"]?.AsArray()) ?? throw new NullReferenceException("GitHub release does not have an assets property.");
            Melon<Mod>.Logger.Msg($"Found assets array: {assetsResponseObj}");

            var outputPath = Path.Combine(Mod.cementBinPath, releaseName.GetValue<string>());
            Directory.CreateDirectory(outputPath);
            Melon<Mod>.Logger.Msg($"Created folder at {outputPath} in preparation for saving release assets to disk. . .");

            foreach (var asset in assetsResponseObj)
            {
                if (asset is null) continue;

                var assetName = asset["name"]?.AsValue().GetValue<string>();
                if (assetName is null) continue;

                Melon<Mod>.Logger.Msg($"Found individual release asset name: {assetName}");

                var downloadToPath = Path.Combine(outputPath, assetName);

                try
                {
                    var assetExtension = Path.GetExtension(assetName).ToLower();
                    if (assetExtension != ".dll")
                    {
                        Melon<Mod>.Logger.Warning("Release asset does not have a valid file extension (.dll)! Its contents will not be downloaded.");
                        continue;
                    }

                    var downloadAssetUrl = asset["browser_download_url"]?.AsValue();
                    if (downloadAssetUrl is null || downloadAssetUrl.GetValue<string>() is null) continue;

                    Melon<Mod>.Logger.Msg($"Found browser download url: {downloadAssetUrl}");
                    Melon<Mod>.Logger.Msg($"Downloading asset {assetName} from {downloadAssetUrl}. . .");

                    var downloadedBytes = await Mod.updaterClient.GetByteArrayAsync(downloadAssetUrl.GetValue<string>());
                    if (downloadedBytes is null || downloadedBytes.Length == 0) continue;

                    File.WriteAllBytes(MelonEnvironment.ModsDirectory, downloadedBytes);
                }
                catch
                {
                    if (File.Exists(downloadToPath)) File.Delete(downloadToPath);
                    throw;
                }
            }

            Melon<Mod>.Logger.Msg($"Successfully downloaded assets from mod release link ({melonToUpdate.Info.DownloadLink}). Output files stored in {outputPath}");
        }
        catch (Exception ex)
        {
            Melon<Mod>.Logger.Error($"An error occured downloading update for mod {melonToUpdate.Info.Name} from GitHub Releases. Update will not continue. Download link: {melonToUpdate.Info.DownloadLink} Error: ", ex);
        }

        // Restart game
        Melon<Mod>.Logger.Msg("mod updates were made, rebooting game. . .");
        Application.Quit();
        Process.Start(MelonEnvironment.GameExecutablePath);
    }
}
