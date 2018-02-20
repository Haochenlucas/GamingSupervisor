using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for ConfirmSelection.xaml
    /// </summary>
    public partial class ConfirmSelection : Page
    {
        public ConfirmSelection()
        {
            InitializeComponent();
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            WaitForCompletion waitForCompletion = new WaitForCompletion();
            navService.Navigate(waitForCompletion);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            if (GUISelection.fileName == null)
            {
                GameTypeSelection gameTypeSelection = new GameTypeSelection();
                navService.Navigate(gameTypeSelection);
            }
            else
            {
                ReplayHeroSelection replayHeroSelection = new ReplayHeroSelection();
                navService.Navigate(replayHeroSelection);
            }
        }
    }
}
