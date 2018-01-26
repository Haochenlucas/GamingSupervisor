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
            GamingSupervisorManager manager = new GamingSupervisorManager();
            manager.Start();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            GameTypeSelection gameTypeSelection = new GameTypeSelection(selection);
            navService.Navigate(gameTypeSelection);
        }
    }
}
