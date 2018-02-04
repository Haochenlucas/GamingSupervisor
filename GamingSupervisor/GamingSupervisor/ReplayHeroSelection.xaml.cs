using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    public class HeroNameItem
    {
        public Image Picture { get; set; }
        public string Title { get; set; }
    }

    /// <summary>
    /// Interaction logic for ReplayHeroSelection.xaml
    /// </summary>
    public partial class ReplayHeroSelection : Page
    {
        private BackgroundWorker worker;
        private List<HeroNameItem> heros;

        public ReplayHeroSelection()
        {
            InitializeComponent();

            ConfirmButton.IsEnabled = false;

            ParsingMessageLabel.Visibility = Visibility.Visible;
            LoadingIcon.Visibility = Visibility.Visible;
            ConfirmButton.Visibility = Visibility.Hidden;
            GoBackButton.Visibility = Visibility.Hidden;

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(StartParsing);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FinishedParsing);
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync();
        }

        private void StartParsing(object sender, DoWorkEventArgs e)
        {
            ParserHandler parser = new ParserHandler(GUISelection.fileName);
            List<string> heroNameList = parser.ParseReplayFile();

            replayParse.heroID heroId = new replayParse.heroID();
            heros = new List<HeroNameItem>();
            foreach (string heroName in heroNameList)
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = new Uri("pack://application:,,,/hero_icon_images/" + replayParse.heroID.ID_heroDictionary[heroName] + ".png");
                image.EndInit();
                Image image2 = new Image();
                image2.Source = image;
                Console.WriteLine(replayParse.heroID.ID_heroDictionary[heroName]);
                heros.Add(new HeroNameItem()
                {
                    Picture = image2,
                    Title = heroName
                });
            }            
        }

        private void FinishedParsing(object sender, RunWorkerCompletedEventArgs e)
        {
            HeroNameListBox.ItemsSource = heros;

            ParsingMessageLabel.Visibility = Visibility.Hidden;
            LoadingIcon.Visibility = Visibility.Hidden;
            ConfirmButton.Visibility = Visibility.Visible;
            GoBackButton.Visibility = Visibility.Visible;
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeroNameListBox.SelectedItem != null)
            {
                ConfirmButton.IsEnabled = true;
                GUISelection.heroName = (HeroNameListBox.SelectedItem as HeroNameItem).Title;
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
            GameTypeSelection gameTypeSelection = new GameTypeSelection();
            navService.Navigate(gameTypeSelection);
        }
    }
}
