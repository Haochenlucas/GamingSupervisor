using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
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

            MainWindow.Description.Text = "Welcome to the Gaming Supervisor! The Gaming Supervisor will help you analyze your replays and play games in DotA 2.";
            MainWindow.Description.Text += "\n\n";
            MainWindow.Description.Text += "Select your experience level. If you have only limited experience, select 'Novice'.";
            MainWindow.Description.Text += " If you have a few more games under your belt, select 'Learning'.";
            MainWindow.Description.Text += " If you think you are almost ready to play without the Gaming Supervisor, select 'Almost got it'.";
        }


        private void SelectNovice(object sender, RoutedEventArgs e)
        {
            GUISelection.difficulty = GUISelection.Difficulty.novice;

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
            GUISelection.difficulty = GUISelection.Difficulty.learning;

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
            GUISelection.difficulty = GUISelection.Difficulty.experienced;

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
