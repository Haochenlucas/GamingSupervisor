using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for ReplaySelection.xaml
    /// </summary>
    public partial class ReplaySelection : Page
    {
        private class ReplayListItem
        {
            public string Title { get; set; }
        }

        private BackgroundWorker worker;

        public ReplaySelection()
        {
            InitializeComponent();

            MainWindow.Description.Text = "Select which replay to analyze.";

            ConfirmButton.IsEnabled = false;

            ParsingMessageLabel.Visibility = Visibility.Visible;
            LoadingIcon.Visibility = Visibility.Visible;
            ConfirmButton.Visibility = Visibility.Hidden;
            GoBackButton.Visibility = Visibility.Hidden;

            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(WaitForParsing);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FinishedParsing);
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync();
        }

        private void WaitForParsing(object sender, DoWorkEventArgs e)
        {
            ParserHandler.WaitForParsing();
        }

        private void FinishedParsing(object sender, RunWorkerCompletedEventArgs e)
        {
            List<ReplayListItem> replays = new List<ReplayListItem>();
            foreach (string replay in
                Directory.EnumerateDirectories(Path.Combine(Environment.CurrentDirectory, "../../Parser/")))
            {
                replays.Add(new ReplayListItem()
                {
                    Title = Path.GetFileName(replay)
                });
            }

            ReplayNameListBox.ItemsSource = replays;

            ParsingMessageLabel.Visibility = Visibility.Hidden;
            LoadingIcon.Visibility = Visibility.Hidden;
            ConfirmButton.Visibility = Visibility.Visible;
            GoBackButton.Visibility = Visibility.Visible;
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReplayNameListBox.SelectedItem != null)
            {
                ConfirmButton.IsEnabled = true;
                GUISelection.fileName = (ReplayNameListBox.SelectedItem as ReplayListItem).Title;
            }
            else
            {
                ConfirmButton.IsEnabled = false;
            }
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            GUISelection.replayDataFolderLocation = Path.Combine("../../Parser", GUISelection.fileName + "/");

            NavigationService navService = NavigationService.GetNavigationService(this);
            ReplayHeroSelection replayHeroSelection = new ReplayHeroSelection();
            navService.Navigate(replayHeroSelection);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            GameTypeSelection gameTypeSelection = new GameTypeSelection();
            navService.Navigate(gameTypeSelection);
        }
    }
}
