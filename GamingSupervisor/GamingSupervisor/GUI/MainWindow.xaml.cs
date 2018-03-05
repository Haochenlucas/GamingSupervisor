using MahApps.Metro;
using MahApps.Metro.Controls;
using System.Windows.Controls;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static TextBlock Description;

        public MainWindow()
        {
            InitializeComponent();

            Description = GUIDescription;

            DifficultySelection difficultySelection = new DifficultySelection();
            GUINavigation.Navigate(difficultySelection);
        }
    }
}
