using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for ReplayHeroSelection.xaml
    /// </summary>
    public partial class ReplayHeroSelection : Page
    {
        private class HeroListItem
        {
            public string ImagePath { get; set; }
            public string Title { get; set; }
        }

        public ReplayHeroSelection()
        {
            InitializeComponent();

            MainWindow.Description.Text = "Select which hero to analyze.";

            List<string> heroNameList = ParserHandler.GetHeroNameList(GUISelection.replayDataFolderLocation);

            replayParse.heroID heroId = new replayParse.heroID();
            List<HeroListItem> heros = new List<HeroListItem>();
            foreach (string heroName in heroNameList)
            {
                heros.Add(new HeroListItem()
                {
                    ImagePath = Path.Combine(Environment.CurrentDirectory, @"..\..\hero_icon_images\" + replayParse.heroID.ID_heroDictionary[heroName].ToString() + ".png"),
                    Title = heroName
                });
            }

            HeroNameListBox.ItemsSource = heros;
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeroNameListBox.SelectedItem != null)
            {
                ConfirmButton.IsEnabled = true;
                GUISelection.heroName = (HeroNameListBox.SelectedItem as HeroListItem).Title;
            }
            else
            {
                ConfirmButton.IsEnabled = false;
            }
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            ConfirmSelection confirmSelection = new ConfirmSelection();
            navService.Navigate(confirmSelection);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            ReplaySelection replaySelection = new ReplaySelection();
            navService.Navigate(replaySelection);
        }
    }
}
