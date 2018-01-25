using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for CustomizeSelection.xaml
    /// </summary>
    public partial class CustomizeSelection : Page
    {
        private GUISelection selection;

        public CustomizeSelection()
        {
            InitializeComponent();
        }

        public CustomizeSelection(GUISelection selection) : this()
        {
            this.selection = selection;

            LastHitListBoxItem.IsSelected = selection.customize[GUISelection.Customize.lastHit];
            HeroSelectionListBoxItem.IsSelected = selection.customize[GUISelection.Customize.heroSelection];
            ItemHelperListBoxItem.IsSelected = selection.customize[GUISelection.Customize.itemHelper];
            LaningListBoxItem.IsSelected = selection.customize[GUISelection.Customize.laning];
            JunglingListBoxItem.IsSelected = selection.customize[GUISelection.Customize.jungling];
            SafeFarmingAreaListBoxItem.IsSelected = selection.customize[GUISelection.Customize.safeFarming];
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);

            selection.customize[GUISelection.Customize.lastHit] = LastHitListBoxItem.IsSelected;
            selection.customize[GUISelection.Customize.heroSelection] = HeroSelectionListBoxItem.IsSelected;
            selection.customize[GUISelection.Customize.itemHelper] = ItemHelperListBoxItem.IsSelected;
            selection.customize[GUISelection.Customize.laning] = LaningListBoxItem.IsSelected;
            selection.customize[GUISelection.Customize.jungling] = JunglingListBoxItem.IsSelected;
            selection.customize[GUISelection.Customize.safeFarming] = SafeFarmingAreaListBoxItem.IsSelected;

            GameTypeSelection gameTypeSelection = new GameTypeSelection(selection);
            navService.Navigate(gameTypeSelection);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            DifficultySelection difficultySelection = new DifficultySelection(selection);
            navService.Navigate(difficultySelection);
        }
    }
}
