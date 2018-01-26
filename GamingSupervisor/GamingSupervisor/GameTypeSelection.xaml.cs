using Microsoft.Win32;
using System;
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
        private GUISelection selection;

        public GameTypeSelection()
        {
            InitializeComponent();
        }

        public GameTypeSelection(GUISelection selection) : this()
        {
            this.selection = selection;
        }

        private void SelectLive(object sender, RoutedEventArgs e)
        {
            selection.fileName = null;
            selection.gameType = GUISelection.GameType.live;

            NavigationService navService = NavigationService.GetNavigationService(this);
            ConfirmSelection confirmSelection = new ConfirmSelection(selection);
            navService.Navigate(confirmSelection);
        }

        private void SelectReplay(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".dem";
            dlg.Filter = "DEM Files (*.dem)|*.dem";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                selection.fileName = dlg.FileName;
                selection.gameType = GUISelection.GameType.replay;

                NavigationService navService = NavigationService.GetNavigationService(this);
                ReplayHeroSelection replayHeroSelection = new ReplayHeroSelection(selection);
                navService.Navigate(replayHeroSelection);
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            CustomizeSelection customizeSelection = new CustomizeSelection(selection);
            navService.Navigate(customizeSelection);
        }
    }
}
