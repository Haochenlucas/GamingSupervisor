﻿using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static TextBlock Description;
        public static ItemsControl HeroList;

        public MainWindow()
        {
            if (Process.GetProcessesByName("GamingSupervisor").Length != 1)
            {
                MessageBox.Show(this, "Gaming Supervisor is already running. Close the other instance and try again.", "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information,
                    MessageBoxResult.OK,
                    MessageBoxOptions.DefaultDesktopOnly);

                    Application.Current.Shutdown();
            }

            InitializeComponent();

            HideInstructions();
            DescriptionToggle.IsChecked = false;

            Description = GUIDescription;
            HeroList = HeroNameItemsControl;
            HeroList.Visibility = Visibility.Collapsed;

            ParserHandler.StartInfoParsing();

            DifficultySelection difficultySelection = new DifficultySelection();
            GUINavigation.Navigate(difficultySelection);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }

        private void HideInstructions()
        {
            this.Width = 360;
            GUIDescriptionGrid.Visibility = Visibility.Hidden;
        }

        private void ShowInstructions()
        {
            this.Width = 960;
            GUIDescriptionGrid.Visibility = Visibility.Visible;
        }

        private void DescriptionToggle_Checked(object sender, RoutedEventArgs e)
        {
            ShowInstructions();
        }

        private void DescriptionToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            HideInstructions();
        }
    }
}
