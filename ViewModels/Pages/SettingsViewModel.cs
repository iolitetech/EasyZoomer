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

        [ObservableProperty]
        private Visibility _isCheckUpdateVisible = Visibility.Visible;

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
            IsUpdateAvailable = Visibility.Hidden;
            _updateInfo = await _updateManager.CheckForUpdatesAsync();

            try
            {
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
                    if (release.Version.ToString() is not null)
                    {
                        IsCheckUpdateVisible = Visibility.Hidden;
                        IsUpdateAvailable = Visibility.Visible;

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
            }
            catch (Exception ex) 
            {
                Wpf.Ui.Controls.MessageBox msgBox = new()
                {
                    Title = "EasyZoomer Update",
                    Content = ex.Message
                };
                await msgBox.ShowDialogAsync();
            }
            finally
            {
                IsProgressVisible = Visibility.Hidden;
                LastChecked = DateTime.Now;
                LastCheckedMessage = $"Last Checked {LastChecked}";
            }
        }

        [RelayCommand]
        public async Task DownloadAndInstall()
        {
            try
            {
                IsProgressVisible = Visibility.Visible;
                await DownloadUpdate();
                InstallUpdate();
            }
            catch (Exception ex)
            {
                Wpf.Ui.Controls.MessageBox msgBox = new()
                {
                    Title = "EasyZoomer Update",
                    Content = ex.Message
                };
                await msgBox.ShowDialogAsync();
            }
            finally
            {
                IsProgressVisible = Visibility.Hidden;
            }
            
        }
        public SettingsViewModel()
        {

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
            var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            return $"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}"
                ?? String.Empty;
        }

        public async Task DownloadUpdate()
        {
            if (_updateInfo != null)
            {
                await _updateManager.DownloadUpdatesAsync(_updateInfo);
                IsUpdateAvailable = Visibility.Collapsed;
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
