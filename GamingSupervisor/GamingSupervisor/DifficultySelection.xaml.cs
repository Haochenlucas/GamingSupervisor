using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for DifficultySelection.xaml
    /// </summary>
    public partial class DifficultySelection : Page
    {
        public DifficultySelection()
        {
            InitializeComponent();
        }

        private void SelectNovice(object sender, RoutedEventArgs e)
        {
            GUISelection.customize[GUISelection.Customize.lastHit] = true;
            GUISelection.customize[GUISelection.Customize.heroSelection] = true;
            GUISelection.customize[GUISelection.Customize.itemHelper] = true;
            GUISelection.customize[GUISelection.Customize.laning] = true;
            GUISelection.customize[GUISelection.Customize.jungling] = true;
            GUISelection.customize[GUISelection.Customize.safeFarming] = true;

            gotoCustomizeSelection();
        }

        private void SelectLearning(object sender, RoutedEventArgs e)
        {
            GUISelection.customize[GUISelection.Customize.lastHit] = true;
            GUISelection.customize[GUISelection.Customize.heroSelection] = false;
            GUISelection.customize[GUISelection.Customize.itemHelper] = false;
            GUISelection.customize[GUISelection.Customize.laning] = true;
            GUISelection.customize[GUISelection.Customize.jungling] = true;
            GUISelection.customize[GUISelection.Customize.safeFarming] = true;

            gotoCustomizeSelection();
        }

        private void SelectAlmostGotIt(object sender, RoutedEventArgs e)
        {
            GUISelection.customize[GUISelection.Customize.lastHit] = false;
            GUISelection.customize[GUISelection.Customize.heroSelection] = false;
            GUISelection.customize[GUISelection.Customize.itemHelper] = false;
            GUISelection.customize[GUISelection.Customize.laning] = false;
            GUISelection.customize[GUISelection.Customize.jungling] = true;
            GUISelection.customize[GUISelection.Customize.safeFarming] = true;

            gotoCustomizeSelection();
        }

        private void gotoCustomizeSelection()
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            CustomizeSelection customizeSelection = new CustomizeSelection();
            navService.Navigate(customizeSelection);
        }
    }
}
