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
        public GameTypeSelection()
        {
            InitializeComponent();
        }

        private void SelectLive(object sender, RoutedEventArgs e)
        {
            GUISelection.fileName = null;
            GUISelection.gameType = GUISelection.GameType.live;

            NavigationService navService = NavigationService.GetNavigationService(this);
            ConfirmSelection confirmSelection = new ConfirmSelection();
            navService.Navigate(confirmSelection);
        }

        private void SelectReplay(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = ".dem";
            dialog.Filter = "DEM Files (*.dem)|*.dem";

            //RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            //if (regKey != null)
            //{
            //    dialog.InitialDirectory = regKey.GetValue("SteamPath") + @"\steamapps\common\dota 2 beta\game\dota\replays\";                
            //}

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
            {
                GUISelection.fileName = dialog.FileName;
                GUISelection.gameType = GUISelection.GameType.replay;

                NavigationService navService = NavigationService.GetNavigationService(this);
                ReplayHeroSelection replayHeroSelection = new ReplayHeroSelection();
                navService.Navigate(replayHeroSelection);
            }
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            CustomizeSelection customizeSelection = new CustomizeSelection();
            navService.Navigate(customizeSelection);
        }
    }
}
