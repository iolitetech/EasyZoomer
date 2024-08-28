using EasyZoomer.ViewModels.Pages;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace EasyZoomer.Views.Windows
{
    public partial class ZoomerWindow : Window
    {
        private double _scale = 1.0;
        private double _circleRadius = 50;
        private bool _isOverlayVisible = false;
        private DashboardViewModel _viewModel;
        private System.Windows.Point _lastMousePosition;

        public ZoomerWindow(DashboardViewModel model)
        {
            InitializeComponent();
            _viewModel = model;
            _scale = model.ZoomScaleFactor;
            _circleRadius = model.CircleRadius;

            Loaded += Zoomer_Loaded;
            WindowState = WindowState.Maximized;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;

            MouseMove += OnMouseMove;
            KeyDown += OnKeyDown;
            MouseWheel += OnMouseWheel;
        }

        private void Zoomer_Loaded(object sender, RoutedEventArgs e)
        {
            CapturedImage.Source = BitmapToImageSource(CaptureScreen());
            UpdateOverlaySize();
        }

        private Bitmap CaptureScreen()
        {
            var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);

            float dpiX = g.DpiX;
            float dpiY = g.DpiY;

            int width = (int)(SystemParameters.VirtualScreenWidth * dpiX / 96.0);
            int height = (int)(SystemParameters.VirtualScreenHeight * dpiY / 96.0);
            int left = (int)(SystemParameters.VirtualScreenLeft * dpiX / 96.0);
            int top = (int)(SystemParameters.VirtualScreenTop * dpiY / 96.0);

            var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bitmapGraphics = System.Drawing.Graphics.FromImage(bitmap);
            bitmapGraphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height), System.Drawing.CopyPixelOperation.SourceCopy);

            return bitmap;
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                AdjustCircleRadius(e.Delta > 0 ? 5 : -5);
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                AdjustZoom(e.Delta > 0 ? 0.1 : -0.1, e.GetPosition(CapturedImage));
            }
        }

        private void AdjustCircleRadius(double change)
        {
            _circleRadius = Math.Max(10, _circleRadius + change);
            UpdateOverlayMask(GetMousePosition());
        }

        private void AdjustZoom(double change, System.Windows.Point position)
        {
            _scale = Math.Max(1.0, _scale + change);

            CapturedImage.RenderTransformOrigin = new System.Windows.Point(
                position.X / CapturedImage.ActualWidth,
                position.Y / CapturedImage.ActualHeight
            );
            CapturedImage.RenderTransform = new ScaleTransform(_scale, _scale);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(OverlayCanvas);

            if (_isOverlayVisible)
            {
                UpdateOverlayMask(mousePosition);
            }

            _lastMousePosition = mousePosition;
        }

        private void UpdateOverlaySize()
        {
            if (OverlayCanvas != null)
            {
                var overlayRectangle = OverlayCanvas.Children.OfType<Rectangle>().FirstOrDefault();
                if (overlayRectangle != default)
                {
                    overlayRectangle.Width = (int)OverlayCanvas.ActualWidth;
                    overlayRectangle.Height = (int)OverlayCanvas.ActualHeight;
                }
            }
        }

        private void UpdateOverlayMask(System.Windows.Point mousePosition)
        {
            if (!_isOverlayVisible) return;

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var overlayRect = new RectangleGeometry(new Rect(0, 0, OverlayCanvas.ActualWidth, OverlayCanvas.ActualHeight));
                var circleGeometry = new EllipseGeometry(mousePosition, _circleRadius, _circleRadius);
                var combinedGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, overlayRect, circleGeometry);

                drawingContext.DrawGeometry(_viewModel.OverlayColorBrush, null, combinedGeometry);
                drawingContext.DrawEllipse(null, new System.Windows.Media.Pen(_viewModel.BorderColorBrush, _viewModel.BorderThickness), mousePosition, _circleRadius, _circleRadius);
            }

            var renderTargetBitmap = new RenderTargetBitmap(
                (int)OverlayCanvas.ActualWidth,
                (int)OverlayCanvas.ActualHeight,
                96, 96, PixelFormats.Pbgra32);

            renderTargetBitmap.Render(drawingVisual);
            OverlayCanvas.Background = new ImageBrush(renderTargetBitmap);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.O when Keyboard.Modifiers.HasFlag(ModifierKeys.Shift):
                    ToggleOverlayVisibility();
                    break;
                case Key.Q:
                    Close();
                    break;
                case Key.Add or Key.OemPlus:
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        AdjustZoom(0.1, GetMousePosition());
                    }
                    else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        AdjustCircleRadius(5);
                    }
                    break;
                case Key.Subtract or Key.OemMinus:
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        AdjustZoom(-0.1, GetMousePosition());
                    }
                    else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        AdjustCircleRadius(-5);
                    }
                    break;
                case Key.D0 when Keyboard.Modifiers.HasFlag(ModifierKeys.Control):
                    ResetZoom();
                    break;
            }
        }

        private void ResetZoom()
        {
            _scale = 1.0; // Reset the zoom scale to 100%
            var scaleTransform = new ScaleTransform(_scale, _scale);
            CapturedImage.RenderTransform = scaleTransform;
            CapturedImage.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        }

        private void ToggleOverlayVisibility()
        {
            _isOverlayVisible = !_isOverlayVisible;
            OverlayCanvas.Visibility = _isOverlayVisible ? Visibility.Visible : Visibility.Hidden;

            if (_isOverlayVisible)
            {
                UpdateOverlayMask(GetMousePosition());
            }
            else
            {
                OverlayCanvas.Children.Clear();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private System.Windows.Point GetMousePosition() => Mouse.GetPosition(OverlayCanvas);
    }
}
