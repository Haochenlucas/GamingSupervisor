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
        private GUISelection selection;

        public ConfirmSelection()
        {
            InitializeComponent();
        }

        public ConfirmSelection(GUISelection selection) : this()
        {
            this.selection = selection;
        }

        private void Go(object sender, RoutedEventArgs e)
        {
            GamingSupervisorManager manager = new GamingSupervisorManager(selection);
            manager.Start();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            if (selection.fileName == null)
            {
                GameTypeSelection gameTypeSelection = new GameTypeSelection(selection);
                navService.Navigate(gameTypeSelection);
            }
            else
            {
                ReplayHeroSelection replayHeroSelection = new ReplayHeroSelection(selection);
                navService.Navigate(replayHeroSelection);
            }
        }
    }
}
