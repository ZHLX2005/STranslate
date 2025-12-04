using Microsoft.Extensions.Logging;
using STranslate.Plugin;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Velopack;
using Velopack.Sources;

namespace STranslate.Core;

public class UpdaterService(
    ILogger<UpdaterService> logger,
    IHttpService httpService,
    INotification notification
    )
{
    private SemaphoreSlim UpdateLock { get; } = new SemaphoreSlim(1);

    public async Task UpdateAppAsync(bool silentUpdate = true)
    {
        await UpdateLock.WaitAsync().ConfigureAwait(false);
        try
        {
            //if (!silentUpdate)
            //    _api.ShowMsg(_api.GetTranslation("pleaseWait"),
            //        _api.GetTranslation("update_flowlauncher_update_check"));

            var updateManager = await GitHubUpdateManagerAsync(httpService, Constant.GitHub).ConfigureAwait(false);

            // UpdateApp CheckForUpdate will return value only if the app is squirrel installed
            var newUpdateInfo = await updateManager.CheckForUpdatesAsync().NonNull().ConfigureAwait(false);

            var newReleaseVersion =
                SemanticVersioning.Version.Parse(newUpdateInfo!.TargetFullRelease.Version.ToString());
            var currentVersion = SemanticVersioning.Version.Parse(Constant.Version);

            logger.LogInformation($"Future Release <{Formatted(newUpdateInfo.TargetFullRelease)}>");

            if (newReleaseVersion <= currentVersion)
            {
                //if (!silentUpdate)
                //    _api.ShowMsgBox(_api.GetTranslation("update_flowlauncher_already_on_latest"));
                return;
            }

            //if (!silentUpdate)
            //    _api.ShowMsg(_api.GetTranslation("update_flowlauncher_update_found"),
            //        _api.GetTranslation("update_flowlauncher_updating"));

            await updateManager.DownloadUpdatesAsync(newUpdateInfo).ConfigureAwait(false);

            await updateManager.WaitExitThenApplyUpdatesAsync(newUpdateInfo.TargetFullRelease).ConfigureAwait(false);

            //if (DataLocation.PortableDataLocationInUse())
            //{
            //    var targetDestination = updateManager.RootAppDirectory +
            //                            $"\\app-{newReleaseVersion}\\{DataLocation.PortableFolderName}";
            //    FilesFolders.CopyAll(DataLocation.PortableDataPath, targetDestination, (s) => _api.ShowMsgBox(s));
            //    if (!FilesFolders.VerifyBothFolderFilesEqual(DataLocation.PortableDataPath, targetDestination,
            //            (s) => _api.ShowMsgBox(s)))
            //        _api.ShowMsgBox(string.Format(
            //            _api.GetTranslation("update_flowlauncher_fail_moving_portable_user_profile_data"),
            //            DataLocation.PortableDataPath,
            //            targetDestination));
            //}
            //else
            //{
            //    await updateManager.CreateUninstallerRegistryEntry().ConfigureAwait(false);
            //}

            var newVersionTips = NewVersionTips(newReleaseVersion.ToString());

            logger.LogInformation($"Update success:{newVersionTips}");

            //if (_api.ShowMsgBox(newVersionTips, _api.GetTranslation("update_flowlauncher_new_update"),
            //        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //    UpdateManager.RestartApp(Constant.ApplicationFileName);
            //}
        }
        catch (Exception e)
        {
            if (e is HttpRequestException or WebException or SocketException ||
                e.InnerException is TimeoutException)
            {
                logger.LogError(e, $"Check your connection and proxy settings to github-cloud.s3.amazonaws.com.");
            }
            else
            {
                logger.LogError(e, $"Error Occurred");
            }

            //if (!silentUpdate)
            //    _api.ShowMsgError(_api.GetTranslation("update_flowlauncher_fail"),
            //        _api.GetTranslation("update_flowlauncher_check_connection"));
        }
        finally
        {
            UpdateLock.Release();
        }
    }

    private class GithubRelease
    {
        [JsonPropertyName("prerelease")] public bool Prerelease { get; set; }

        [JsonPropertyName("published_at")] public DateTime PublishedAt { get; set; }

        [JsonPropertyName("html_url")] public string HtmlUrl { get; set; } = string.Empty;
    }

    // https://github.com/Squirrel/Squirrel.Windows/blob/master/src/Squirrel/UpdateManager.Factory.cs
    private static async Task<UpdateManager> GitHubUpdateManagerAsync(IHttpService httpService,string repository)
    {
        var uri = new Uri(repository);
        var api = $"https://api.github.com/repos{uri.AbsolutePath}/releases";

        var jsonStream = await httpService.GetAsStreamAsync(api, CancellationToken.None).ConfigureAwait(false);
        var releases = await JsonSerializer.DeserializeAsync<List<GithubRelease>>(jsonStream).ConfigureAwait(false);
        var latest = releases.Where(r => !r.Prerelease).OrderByDescending(r => r.PublishedAt).First();
        var latestUrl = latest.HtmlUrl.Replace("/tag/", "/download/");

        //var client = new WebClient { Proxy = Http.WebProxy };
        //var downloader = new FileDownloader(client);

        //var manager = new UpdateManager(latestUrl, urlDownloader: downloader);
        var manager = new UpdateManager(latestUrl);

        return manager;
    }

    private string NewVersionTips(string version)
    {
        var tips = string.Format(_api.GetTranslation("newVersionTips"), version);

        return tips;
    }

    private static string Formatted<T>(T t)
    {
        var formatted = JsonSerializer.Serialize(t, new JsonSerializerOptions { WriteIndented = true });

        return formatted;
    }
}