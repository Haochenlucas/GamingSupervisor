using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for VisualCustomize.xaml
    /// </summary>
    public partial class VisualCustomize : Window
    {
        private readonly double aspectRatio;

        public readonly double ScreenHeight;
        public readonly double ScreenWidth;


        public VisualCustomize()
        {
            InitializeComponent();

            aspectRatio = SystemParameters.PrimaryScreenHeight / SystemParameters.PrimaryScreenWidth;
            ScreenHeight = SystemParameters.PrimaryScreenHeight + SystemParameters.WindowCaptionHeight;
            ScreenWidth = SystemParameters.PrimaryScreenWidth;

            Height = ScreenHeight * 3 / 4; // Arbitrarily assign the height of the window to 3/4 the height of the screen
            Width = Height / aspectRatio;

            CustomizeCanvas.Height = Height;
            CustomizeCanvas.Width = Width;
        }

        public void AddElement(ContentControl contentControl, int positionX, int positionY)
        {
            contentControl.Template = FindResource("DesignerItemTemplate") as ControlTemplate;

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Black),
                IsHitTestVisible = false
            };
            contentControl.Content = rectangle;

            CustomizeCanvas.Children.Add(contentControl);

            Canvas.SetLeft(contentControl, positionX);
            Canvas.SetTop(contentControl, positionY);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            double percentWidthChange = (sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / sizeInfo.PreviousSize.Width;
            double percentHeightChange = (sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / sizeInfo.PreviousSize.Height;

            double oldWidth = CustomizeCanvas.Width;
            double oldHeight = CustomizeCanvas.Height;

            if (Math.Abs(percentWidthChange) > Math.Abs(percentHeightChange))
            {
                if (WindowState == WindowState.Maximized)
                {
                    oldWidth = CustomizeCanvas.Width;
                    CustomizeCanvas.Height = ActualHeight;
                    CustomizeCanvas.Width = ActualHeight / aspectRatio;
                    percentWidthChange = (CustomizeCanvas.Width - oldWidth) / oldWidth;
                }
                else
                {
                    CustomizeCanvas.Height = ActualWidth * aspectRatio;
                    CustomizeCanvas.Width = ActualWidth;
                }
                foreach (var c in CustomizeCanvas.Children)
                {
                    if (c.GetType() == typeof(ContentControl))
                    {
                        ContentControl child = c as ContentControl;
                        double childAspectRatio = child.Height / child.Width;
                        child.Width *= 1 + percentWidthChange;
                        child.Height = child.Width * childAspectRatio;

                        Canvas.SetLeft(child, Canvas.GetLeft(child) / oldWidth * CustomizeCanvas.Width);
                        Canvas.SetTop(child, Canvas.GetTop(child) / oldHeight * CustomizeCanvas.Height);
                    }
                }
            }
            else
            {
                CustomizeCanvas.Width = ActualHeight / aspectRatio;
                CustomizeCanvas.Height = ActualHeight;
                foreach (var c in CustomizeCanvas.Children)
                {
                    if (c.GetType() == typeof(ContentControl))
                    {
                        ContentControl child = c as ContentControl;
                        double childAspectRatio = child.Height / child.Width;
                        child.Height *= 1 + percentHeightChange;
                        child.Width = child.Height / childAspectRatio;

                        Canvas.SetLeft(child, Canvas.GetLeft(child) / oldWidth * CustomizeCanvas.Width);
                        Canvas.SetTop(child, Canvas.GetTop(child) / oldHeight * CustomizeCanvas.Height);
                    }
                }
            }

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
