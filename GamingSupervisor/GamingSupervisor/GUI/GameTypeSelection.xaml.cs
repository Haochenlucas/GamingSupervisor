using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for GameTypeSelection.xaml
    /// </summary>
    public partial class GameTypeSelection : Page
    {
        public GameTypeSelection()
        {
            InitializeComponent();

            MainWindow.Description.Text = "Select 'Live' to play a live game with the Gaming Supervisor providing live feedback.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "Select 'Replay' to analyze a replay with the Gaming Supervisor.";
            MainWindow.Description.Text += " You must have played at least one live game or downloaded a replay to select this option.";

            MainWindow.HeroList.Visibility = Visibility.Collapsed;
            MainWindow.Description.Visibility = Visibility.Visible;
        }

        private void SelectLive(object sender, RoutedEventArgs e)
        {
            GUISelection.gameType = GUISelection.GameType.live;

            NavigationService navService = NavigationService.GetNavigationService(this);
            ConfirmSelection confirmSelection = new ConfirmSelection();
            navService.Navigate(confirmSelection);
        }

        private void SelectReplay(object sender, RoutedEventArgs e)
        {
            GUISelection.fileName = null;
            GUISelection.gameType = GUISelection.GameType.replay;

            NavigationService navService = NavigationService.GetNavigationService(this);
            ReplaySelection replaySelection = new ReplaySelection();
            navService.Navigate(replaySelection);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            CustomizeSelection customizeSelection = new CustomizeSelection();
            navService.Navigate(customizeSelection);
        }
    }
}
