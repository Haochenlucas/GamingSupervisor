using MahApps.Metro.Controls;
using System;
using System.Windows;
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

            ParserHandler.StartInfoParsing();

            DifficultySelection difficultySelection = new DifficultySelection();
            GUINavigation.Navigate(difficultySelection);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}
