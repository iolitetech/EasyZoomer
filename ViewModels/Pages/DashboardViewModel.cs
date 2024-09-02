using System.Windows.Input;
using System.Windows.Media;

namespace EasyZoomer.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        // Overlay color properties
        [ObservableProperty]
        private string _overlayColorHex = AppConfig.OverlayColorHex;

        [ObservableProperty]
        private double _overlayOpacity = AppConfig.OverlayOpacity;

        [ObservableProperty]
        private SolidColorBrush _overlayColorBrush;

        // Circle properties
        [ObservableProperty]
        private float _circleRadius = AppConfig.CircleRadius;

        // Border color properties
        [ObservableProperty]
        private string _borderColorHex = AppConfig.BorderColorHex;

        [ObservableProperty]
        private double _borderColorOpacity = AppConfig.BorderColorOpacity;

        [ObservableProperty]
        private SolidColorBrush _borderColorBrush;

        [ObservableProperty]
        private float _borderThickness = AppConfig.BorderThickness;

        // Zoom properties
        [ObservableProperty]
        private float _zoomScaleFactor = AppConfig.ZoomScaleFactor;

        // Constructor
        public DashboardViewModel()
        {
            UpdateOverlayColor();
            UpdateBorderColor();
        }

        // Partial methods for property changes
        partial void OnOverlayColorHexChanged(string value)
        {
            AppConfig.OverlayColorHex = value;
            UpdateOverlayColor();
        }

        partial void OnOverlayOpacityChanged(double value)
        {
            AppConfig.OverlayOpacity = value;
            UpdateOverlayColor();
        }

        partial void OnCircleRadiusChanged(float value)
        {
            AppConfig.CircleRadius = value;
        }

        partial void OnBorderColorHexChanged(string value)
        {
            AppConfig.BorderColorHex = value;
            UpdateBorderColor();
        }

        partial void OnBorderColorOpacityChanged(double value)
        {
            AppConfig.BorderColorOpacity = value;
            UpdateBorderColor();
        }

        partial void OnBorderThicknessChanged(float value)
        {
            AppConfig.BorderThickness = value;
        }

        partial void OnZoomScaleFactorChanged(float value)
        {
            AppConfig.ZoomScaleFactor = value;
        }

        // Update methods for colors
        private void UpdateOverlayColor()
        {
            if (TryConvertHexToColor(OverlayColorHex, out var color))
            {
                color.A = (byte)(OverlayOpacity * 255); // Adjust the alpha channel based on opacity
                OverlayColorBrush = new SolidColorBrush(color);
            }
            else
                OverlayColorBrush = new SolidColorBrush(Color.FromArgb(127,0,0,0));
        }

        private void UpdateBorderColor()
        {
            if (TryConvertHexToColor(BorderColorHex, out var color))
            {
                color.A = (byte)(BorderColorOpacity * 255); // Adjust the alpha channel based on opacity
                BorderColorBrush = new SolidColorBrush(color);
            }
            else
                BorderColorBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        }

        // Utility method to convert hex color string to Color
        private bool TryConvertHexToColor(string hex, out Color color)
        {
            try
            {
                color = (Color)ColorConverter.ConvertFromString(hex);
                return true;
            }
            catch (FormatException)
            {
                color = Colors.Transparent;
                return false;
            }
        }

    }
}
