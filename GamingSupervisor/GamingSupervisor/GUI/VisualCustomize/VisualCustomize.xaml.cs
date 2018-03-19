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
    }
}
