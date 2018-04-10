using Dota2Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        static List<ReplayListItem> replays = null;

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
            worker.DoWork += new DoWorkEventHandler(waitForAsync);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FinishedParsing);
            worker.WorkerReportsProgress = true;

            worker.RunWorkerAsync();
        }

        private void waitForAsync(object sender, DoWorkEventArgs e)
        {
            Task task = WaitForParsingAsync();
            task.Wait();
        }

        private async Task WaitForParsingAsync()
        {
            ParserHandler.WaitForInfoParsing();

            if (replays != null)
                return;

            using (ApiHandler api = new ApiHandler("8BFC2C10E3D1E95B85DCF6AAD861782D"))
            {
                var leagues = await api.GetLeagueListings();

                replays = new List<ReplayListItem>();
                foreach (string replay in
                    Directory.EnumerateDirectories(Path.Combine(Environment.CurrentDirectory, "Parser")))
                {
                    if (!File.Exists(Path.Combine(replay, "info.txt")))
                        continue;

                    string info = File.ReadAllText(Path.Combine(replay, "info.txt"));
                    var matches = Regex.Matches(info, @"match_id: (?<MatchID>\d+)");
                    if (matches.Count == 0)
                        continue;
                    string replayID = matches[0].Groups["MatchID"].Value;
                    Console.WriteLine(replayID);
                    var matchResult = await api.GetDetailedMatch(replayID);

                    string winner = "";
                    switch (matchResult.WinningFaction)
                    {
                        case Dota2Api.Enums.Faction.Dire:
                            winner = "Dire";
                            break;
                        case Dota2Api.Enums.Faction.Radiant:
                            winner = "Radient";
                            break;
                    }

                    TimeSpan time = TimeSpan.FromSeconds(matchResult.Duration);
                    string timeString = time.ToString(@"hh\:mm\:ss");

                    string leagueName = "";
                    /*if (leagues.Leagues.Count != 0)
                    {
                        leagueName = (from league in leagues.Leagues
                                      where league.LeagueId == matchResult.LeagueId
                                      select league.Name).Single();
                    }*/

                    leagueName = leagueName.Replace("#DOTA_Item", "");
                    leagueName = leagueName.Replace("_", " ");
                    leagueName = leagueName == "" ? "" : $"League: {leagueName}\n";

                    string entry = $"{leagueName}Duration: {timeString} Winner: {winner}\nGameID: {matchResult.MatchId}";

                    replays.Add(new ReplayListItem()
                    {
                        Title = entry
                    });
                }
            }
        }

        private void FinishedParsing(object sender, RunWorkerCompletedEventArgs e)
        {
            ReplayNameListBox.ItemsSource = replays;

            GridHolder.Visibility = Visibility.Visible;
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
                string[] words = (ReplayNameListBox.SelectedItem as ReplayListItem).Title.Split(' ');
                GUISelection.fileName = words[words.Length - 1];
            }
            else
            {
                ConfirmButton.IsEnabled = false;
            }
        }

        private void ConfirmSelection(object sender, RoutedEventArgs e)
        {
            GUISelection.replayDataFolderLocation = Path.Combine("Parser", GUISelection.fileName + "/");

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
