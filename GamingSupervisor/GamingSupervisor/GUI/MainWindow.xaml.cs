using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using System;
using System.IO;
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
    }
}
