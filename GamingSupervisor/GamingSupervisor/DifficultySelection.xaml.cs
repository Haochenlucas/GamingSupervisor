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
    /// Interaction logic for DifficultySelection.xaml
    /// </summary>
    public partial class DifficultySelection : Page
    {
        private GUISelection selection;

        public DifficultySelection()
        {
            InitializeComponent();
        }

        public DifficultySelection(GUISelection selection) : this()
        {
            this.selection = selection;
        }

        private void SelectNovice(object sender, RoutedEventArgs e)
        {
            selection.customize[GUISelection.Customize.lastHit] = true;
            selection.customize[GUISelection.Customize.heroSelection] = true;
            selection.customize[GUISelection.Customize.itemHelper] = true;
            selection.customize[GUISelection.Customize.laning] = true;
            selection.customize[GUISelection.Customize.jungling] = true;
            selection.customize[GUISelection.Customize.safeFarming] = true;

            gotoCustomizeSelection();
        }

        private void SelectLearning(object sender, RoutedEventArgs e)
        {
            selection.customize[GUISelection.Customize.lastHit] = true;
            selection.customize[GUISelection.Customize.heroSelection] = false;
            selection.customize[GUISelection.Customize.itemHelper] = false;
            selection.customize[GUISelection.Customize.laning] = true;
            selection.customize[GUISelection.Customize.jungling] = true;
            selection.customize[GUISelection.Customize.safeFarming] = true;

            gotoCustomizeSelection();
        }

        private void SelectAlmostGotIt(object sender, RoutedEventArgs e)
        {
            selection.customize[GUISelection.Customize.lastHit] = false;
            selection.customize[GUISelection.Customize.heroSelection] = false;
            selection.customize[GUISelection.Customize.itemHelper] = false;
            selection.customize[GUISelection.Customize.laning] = false;
            selection.customize[GUISelection.Customize.jungling] = true;
            selection.customize[GUISelection.Customize.safeFarming] = true;

            gotoCustomizeSelection();
        }

        private void gotoCustomizeSelection()
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            CustomizeSelection customizeSelection = new CustomizeSelection(selection);
            navService.Navigate(customizeSelection);
        }
    }
}
