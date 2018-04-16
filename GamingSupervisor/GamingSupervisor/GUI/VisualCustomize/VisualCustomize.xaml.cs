using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GamingSupervisor
{
    public class OverlayBox : ContentControl
    {
        public bool IsOverlayVisible
        {
            get;
            set;
        }

        public OverlayBox() : base()
        {
            IsOverlayVisible = true;
        }
    }

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

            Closing += VisualCustomize_Closing;

            aspectRatio = SystemParameters.PrimaryScreenHeight / SystemParameters.PrimaryScreenWidth;
            ScreenHeight = SystemParameters.PrimaryScreenHeight +
                SystemParameters.WindowCaptionHeight +
                SystemParameters.ResizeFrameHorizontalBorderHeight;
            ScreenWidth = SystemParameters.PrimaryScreenWidth;

            Height = ScreenHeight * 3 / 4; // Arbitrarily assign the height of the window to 3/4 the height of the screen
            Width = Height / aspectRatio;

            CustomizeCanvas.Height = Height;
            CustomizeCanvas.Width = Width;
        }

        public void CloseWindow()
        {
            Closing -= VisualCustomize_Closing;
            Close();
        }

        private void VisualCustomize_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Prevent window from closing
            e.Cancel = true;
        }

        public void AddBox(OverlayBox box, int positionX, int positionY)
        {
            box.Template = FindResource("DesignerItemTemplate") as ControlTemplate;

            Rectangle rectangle = new Rectangle
            {
                Fill = new SolidColorBrush(Colors.Black),
                IsHitTestVisible = false
            };
            box.Content = rectangle;
            box.MouseDoubleClick += Box_MouseDoubleClick;

            CustomizeCanvas.Children.Add(box);

            Canvas.SetLeft(box, positionX);
            Canvas.SetTop(box, positionY);
        }

        private void Box_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OverlayBox box = sender as OverlayBox;
            if (box.IsOverlayVisible)
                box.IsOverlayVisible = false;
            else
                box.IsOverlayVisible = true;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            double percentWidthChange = (sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width) / sizeInfo.PreviousSize.Width;
            double percentHeightChange = (sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height) / sizeInfo.PreviousSize.Height;

            double oldWidth = CustomizeCanvas.Width;
            double oldHeight = CustomizeCanvas.Height;

            if (ActualWidth * aspectRatio >= ActualHeight && Math.Abs(percentWidthChange) > Math.Abs(percentHeightChange))
            {
                // Skip
            }
            else if (Math.Abs(percentWidthChange) > Math.Abs(percentHeightChange))
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
                    if(c.GetType() == typeof(OverlayBox))
                    {
                        OverlayBox box = c as OverlayBox;
                        double childAspectRatio = box.Height / box.Width;
                        box.Width *= 1 + percentWidthChange;
                        box.Height = box.Width * childAspectRatio;

                        Canvas.SetLeft(box, Canvas.GetLeft(box) / oldWidth * CustomizeCanvas.Width);
                        Canvas.SetTop(box, Canvas.GetTop(box) / oldHeight * CustomizeCanvas.Height);
                    }
                }
            }
            else
            {
                CustomizeCanvas.Width = ActualHeight / aspectRatio;
                CustomizeCanvas.Height = ActualHeight;
                foreach (var c in CustomizeCanvas.Children)
                {
                    if (c.GetType() == typeof(OverlayBox))
                    {
                        OverlayBox box = c as OverlayBox;
                        double childAspectRatio = box.Height / box.Width;
                        box.Height *= 1 + percentHeightChange;
                        box.Width = box.Height / childAspectRatio;

                        Canvas.SetLeft(box, Canvas.GetLeft(box) / oldWidth * CustomizeCanvas.Width);
                        Canvas.SetTop(box, Canvas.GetTop(box) / oldHeight * CustomizeCanvas.Height);
                    }
                }
            }

            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}
