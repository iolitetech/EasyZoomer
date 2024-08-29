using System.Collections.ObjectModel;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Velopack;
using Velopack.Sources;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace EasyZoomer.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        private UpdateManager _updateManager;
        private UpdateInfo? _updateInfo;
        private readonly IContentDialogService _contentDialogService;

        [ObservableProperty]
        private DateTime _lastChecked = AppConfig.LastChecked;

        [ObservableProperty]
        private Visibility _isUpdateAvailable = Visibility.Hidden;

        [ObservableProperty]
        private string _lastCheckedMessage = AppConfig.LastCheckedMessage;

        [ObservableProperty]
        private Visibility _isProgressVisible = Visibility.Hidden;

        [ObservableProperty]
        private string _appVersion = String.Empty;

        [ObservableProperty]
        private ApplicationTheme _currentTheme = ApplicationTheme.Unknown;

        [ObservableProperty]
        private ObservableCollection<ThemeOption> _themeOptions;

        [RelayCommand]
        private async Task CheckForUpdateAsync()
        {
            IsProgressVisible = Visibility.Visible;
            _updateInfo = await _updateManager.CheckForUpdatesAsync();

            

            if (_updateInfo is null)
            {
                Wpf.Ui.Controls.MessageBox msgBox = new()
                {
                    Title = "EasyZoomer Update",
                    Content = "EasyZoomer is already up to date."
                };
                await msgBox.ShowDialogAsync();
                
            }
            else
            {
                var release = _updateInfo.TargetFullRelease;
                if(release.Version.ToString() is not null)
                {
                    ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new()
                    {
                        Title = "EasyZoomer update",
                        Content = $"New update available EasyZoomer - {release.Version.ToString()}, Do you want to update?",
                        CloseButtonText = "Close",
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "No"
                    });

                    switch (result)
                    {
                        case ContentDialogResult.Primary:
                            await DownloadUpdate();
                            InstallUpdate();
                            break;

                        case ContentDialogResult.Secondary:
                        case ContentDialogResult.None:
                            IsUpdateAvailable = Visibility.Visible;
                            break;
                    }
                }
                else
                {
                    Wpf.Ui.Controls.MessageBox msgBox = new()
                    {
                        Title = "EasyZoomer Update",
                        Content = "EasyZoomer is already up to date."
                    };
                    await msgBox.ShowDialogAsync();
                }
            }
            IsProgressVisible = Visibility.Hidden;
            LastChecked = DateTime.Now;
            LastCheckedMessage = $"Last Checked {LastChecked}";
        }

        [RelayCommand]
        public async Task DownloadAndInstall()
        {
            await DownloadUpdate();
            InstallUpdate();
        }
        public SettingsViewModel(IContentDialogService dialogService)
        {
            this._contentDialogService = dialogService;

            ThemeOptions = new ObservableCollection<ThemeOption>
            {
                new ThemeOption { DisplayName = "Light", Value = ApplicationTheme.Light },
                new ThemeOption { DisplayName = "Dark", Value = ApplicationTheme.Dark }
            };

            var source = new GithubSource("https://github.com/iolitetech/EasyZoomer", string.Empty, false);
            _updateManager = new UpdateManager(source);
        }

        public void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        public void OnNavigatedFrom() { }

        private void InitializeViewModel()
        {
            CurrentTheme = ApplicationThemeManager.GetAppTheme();
            AppVersion = $"{GetAssemblyVersion()}";

            _isInitialized = true;
        }

        private string GetAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? String.Empty;
        }

        public async Task DownloadUpdate()
        {
            if (_updateInfo != null)
            {
                await _updateManager.DownloadUpdatesAsync(_updateInfo);
                IsUpdateAvailable = Visibility.Visible;
            }
        }
        public void InstallUpdate()
        {
            if (_updateInfo != null)
            {
                _updateManager.ApplyUpdatesAndRestart(_updateInfo);
                IsUpdateAvailable = Visibility.Hidden;
            }
        }
        
        partial void OnCurrentThemeChanged(ApplicationTheme value)
        {
            ApplicationThemeManager.Apply(value);
        }
        partial void OnLastCheckedMessageChanged(string value)
        {
            AppConfig.LastCheckedMessage = value;
        }
        partial void OnLastCheckedChanged(DateTime value)
        {
            AppConfig.LastChecked = value;
        }
    }
  
    public class ThemeOption
    {
        public string DisplayName { get; set; }
        public ApplicationTheme Value { get; set; }
    }
}
