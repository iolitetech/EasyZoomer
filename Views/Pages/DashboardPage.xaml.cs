using EasyZoomer.ViewModels.Pages;
using EasyZoomer.Views.Windows;
using Wpf.Ui.Controls;

namespace EasyZoomer.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardViewModel>
    {
        public DashboardViewModel ViewModel { get; }

        public DashboardPage(DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        private void zoomerOpenButton_Click(object sender, RoutedEventArgs e)
        {
            ZoomerWindow window = new ZoomerWindow(this.ViewModel);
            window.Show();

        }
    }
}
