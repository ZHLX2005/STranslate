using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STranslate.Core;
using STranslate.Services;
using System.Diagnostics;
using System.IO;

namespace STranslate.ViewModels.Pages;

public partial class AboutViewModel(
    Settings settings,
    DataProvider dataProvider,
    BackupService backupService) : ObservableObject
{
    public Settings Settings { get; } = settings;
    public DataProvider DataProvider { get; } = dataProvider;
    public string AppVersion => Constant.Version;

    #region ICommand

    [RelayCommand]
    private void LocateUserData()
    {
        var settingsFolderPath = Path.Combine(DataLocation.SettingsDirectory);
        var parentFolderPath = Path.GetDirectoryName(settingsFolderPath);
        if (Directory.Exists(parentFolderPath))
        {
            Process.Start("explorer.exe", parentFolderPath);
        }
    }

    [RelayCommand]
    private void LocateLog()
    {
        var logFolderPath = Path.Combine(Constant.LogDirectory);
        if (Directory.Exists(logFolderPath))
        {
            Process.Start("explorer.exe", logFolderPath);
        }
    }

    [RelayCommand]
    private void LocateSettings()
    {
        var settingsFolderPath = Path.Combine(DataLocation.SettingsDirectory);
        if (Directory.Exists(settingsFolderPath))
        {
            Process.Start("explorer.exe", settingsFolderPath);
        }
    }

    [RelayCommand]
    private async Task BackupAsync()
    {
        if (Settings.Backup.Type == BackupType.Local)
            await backupService.LocalBackupAsync();
        else
            await backupService.PreWebDavBackupAsync();
    }

    [RelayCommand]
    private async Task RestoreAsync()
    {
        if (Settings.Backup.Type == BackupType.Local)
            await backupService.LocalRestoreAsync();
        else
            await backupService.WebDavRestoreAsync();
    }

    #endregion
}
