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

            MainWindow.Description.Text = "Here you can customize what the Gaming Supervisor will help you do. Setting have already been selected for you based on what difficulty you selected.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "'Last hitting' is where you inflict the last hit on an enemy unit and kill them, which gives you in-game bonuses.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "'Hero selection' takes place at the beginning of the game, and involves selecting which hero you will play with.";
            MainWindow.Description.Text += " The hero you should pick is affected by the heros other players selected, heros the other team won't let you have (called 'banned'), and you skill level.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "The item helper with help you with deciding when to use the items in your inventory.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "In DotA 2, there are three 'lanes': the top lane, the middle lane, and the bottom lane. 'Laning' is when you attack and defend in the three lanes.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "'Jungling' is where you kill neutral monsters in the jungle areas on the map to receive gold and XP.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "'Farming' is where you kill enemy 'creeps' (monsters) to receive gold and XP.";

            LastHitToggle.IsChecked = GUISelection.customize[GUISelection.Customize.lastHit];
            HeroSelectionToggle.IsChecked = GUISelection.customize[GUISelection.Customize.heroSelection];
            ItemHelperToggle.IsChecked = GUISelection.customize[GUISelection.Customize.itemHelper];
            LaningToggle.IsChecked = GUISelection.customize[GUISelection.Customize.laning];
            JunglingToggle.IsChecked = GUISelection.customize[GUISelection.Customize.jungling];
            SafeFarmingAreaToggle.IsChecked = GUISelection.customize[GUISelection.Customize.safeFarming];
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            GUISelection.customize[GUISelection.Customize.lastHit] = (bool)LastHitToggle.IsChecked;
            GUISelection.customize[GUISelection.Customize.heroSelection] = (bool)HeroSelectionToggle.IsChecked;
            GUISelection.customize[GUISelection.Customize.itemHelper] = (bool)ItemHelperToggle.IsChecked;
            GUISelection.customize[GUISelection.Customize.laning] = (bool)LaningToggle.IsChecked;
            GUISelection.customize[GUISelection.Customize.jungling] = (bool)JunglingToggle.IsChecked;
            GUISelection.customize[GUISelection.Customize.safeFarming] = (bool)SafeFarmingAreaToggle.IsChecked;

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
            if (!(bool)LastHitToggle.IsChecked &&
                !(bool)HeroSelectionToggle.IsChecked &&
                !(bool)ItemHelperToggle.IsChecked &&
                !(bool)LaningToggle.IsChecked &&
                !(bool)JunglingToggle.IsChecked &&
                !(bool)SafeFarmingAreaToggle.IsChecked)
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
