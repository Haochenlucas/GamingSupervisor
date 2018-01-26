using System;
using System.Collections.Generic;
using System.IO;
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
    public class HeroNameItem
    {
        public string Title { get; set; }
    }

    /// <summary>
    /// Interaction logic for ReplayHeroSelection.xaml
    /// </summary>
    public partial class ReplayHeroSelection : Page
    {
        private GUISelection selection;

        public ReplayHeroSelection()
        {
            InitializeComponent();
        }

        public ReplayHeroSelection(GUISelection selection) : this()
        {
            this.selection = selection;

            ParserHandler parser = new ParserHandler(selection.fileName);
            List<string> heroNameList = parser.ParseReplayFile();

            List<HeroNameItem> items = new List<HeroNameItem>();
            foreach (string heroName in heroNameList)
            {
                items.Add(new HeroNameItem() { Title = heroName });
            }

            HeroNameListBox.ItemsSource = items;

            ConfirmButton.IsEnabled = false;
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeroNameListBox.SelectedItem != null)
            {
                ConfirmButton.IsEnabled = true;
                selection.heroName = (HeroNameListBox.SelectedItem as HeroNameItem).Title;
            }
            else
            {
                ConfirmButton.IsEnabled = false;
            }
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            ConfirmSelection confirmSelection = new ConfirmSelection(selection);
            navService.Navigate(confirmSelection);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            GameTypeSelection gameTypeSelection = new GameTypeSelection(selection);
            navService.Navigate(gameTypeSelection);
        }
    }
}
