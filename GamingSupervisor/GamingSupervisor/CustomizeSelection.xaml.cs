using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for CustomizeSelection.xaml
    /// </summary>
    public partial class CustomizeSelection : Page
    {
        public CustomizeSelection()
        {
            InitializeComponent();

            LastHitListBoxItem.IsSelected = GUISelection.customize[GUISelection.Customize.lastHit];
            HeroSelectionListBoxItem.IsSelected = GUISelection.customize[GUISelection.Customize.heroSelection];
            ItemHelperListBoxItem.IsSelected = GUISelection.customize[GUISelection.Customize.itemHelper];
            LaningListBoxItem.IsSelected = GUISelection.customize[GUISelection.Customize.laning];
            JunglingListBoxItem.IsSelected = GUISelection.customize[GUISelection.Customize.jungling];
            SafeFarmingAreaListBoxItem.IsSelected = GUISelection.customize[GUISelection.Customize.safeFarming];
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            GUISelection.customize[GUISelection.Customize.lastHit] = LastHitListBoxItem.IsSelected;
            GUISelection.customize[GUISelection.Customize.heroSelection] = HeroSelectionListBoxItem.IsSelected;
            GUISelection.customize[GUISelection.Customize.itemHelper] = ItemHelperListBoxItem.IsSelected;
            GUISelection.customize[GUISelection.Customize.laning] = LaningListBoxItem.IsSelected;
            GUISelection.customize[GUISelection.Customize.jungling] = JunglingListBoxItem.IsSelected;
            GUISelection.customize[GUISelection.Customize.safeFarming] = SafeFarmingAreaListBoxItem.IsSelected;

            NavigationService navService = NavigationService.GetNavigationService(this);
            GameTypeSelection gameTypeSelection = new GameTypeSelection();
            navService.Navigate(gameTypeSelection);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            DifficultySelection difficultySelection = new DifficultySelection();
            navService.Navigate(difficultySelection);
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!LastHitListBoxItem.IsSelected &&
                !HeroSelectionListBoxItem.IsSelected &&
                !ItemHelperListBoxItem.IsSelected &&
                !LaningListBoxItem.IsSelected &&
                !JunglingListBoxItem.IsSelected &&
                !SafeFarmingAreaListBoxItem.IsSelected)
            {
                ConfirmSelectionButton.IsEnabled = false;
            }
            else
            {
                ConfirmSelectionButton.IsEnabled = true;
            }
        }
    }
}
