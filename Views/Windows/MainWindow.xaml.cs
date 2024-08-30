using EasyZoomer.ViewModels.Pages;
using EasyZoomer.ViewModels.Windows;
using NHotkey;
using NHotkey.Wpf;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;


namespace EasyZoomer.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }
        public DashboardViewModel DashboardViewModel { get; }

        private ZoomerWindow? _zoomerWindow;

        public MainWindow(
            MainWindowViewModel viewModel,
            DashboardViewModel dashboardViewModel,
            IPageService pageService,
            INavigationService navigationService
        )
        {
            ViewModel = viewModel;
            DashboardViewModel = dashboardViewModel;
            DataContext = this;


            SystemThemeWatcher.Watch(this);

            InitializeComponent();
            SetPageService(pageService);
            RegisterHotKey();
            navigationService.SetNavigationControl(RootNavigation);
        }

        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService) => RootNavigation.SetPageService(pageService);

        public void ShowWindow() => Show();

        public void CloseWindow() => Close();

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }

        public void OpenZoomerWindow(object sneder, HotkeyEventArgs e)
        {
            if (_zoomerWindow != null && _zoomerWindow.IsVisible)
            {
                _zoomerWindow.Activate(); // Bring the existing window to focus
                return;
            }

            _zoomerWindow = new ZoomerWindow(DashboardViewModel);
            _zoomerWindow.Closed += (s, args) => _zoomerWindow = null; // Reset the reference when the window is closed
            _zoomerWindow.Show();
        }

        public async ValueTask RegisterHotKey()
        {
            try
            {
                HotkeyManager.Current.AddOrReplace("OpenZoomer", Key.O, ModifierKeys.Windows | ModifierKeys.Shift, OpenZoomerWindow);
            }
            catch
            {
                var msgBox = new Wpf.Ui.Controls.MessageBox()
                {
                    Title = "Error register hot key",
                    Content = "Could not add hot key (win + shift + 0), you can still open the zoomer in the app."
                };

                await msgBox.ShowDialogAsync();
            }

        }
        INavigationView INavigationWindow.GetNavigation()
        {
            throw new NotImplementedException();
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }


    }
}
