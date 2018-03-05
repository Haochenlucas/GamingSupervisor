using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for WaitForCompletion.xaml
    /// </summary>
    public partial class WaitForCompletion : Page
    {
        private GamingSupervisorManager manager;
        private BackgroundWorker worker;

        public WaitForCompletion()
        {
            InitializeComponent();

            MainWindow.Description.Text = "Currently analyzing replay. Follow the directions once DotA starts. Open 'Watch', 'Downloads', and select " +
                System.IO.Path.GetFileNameWithoutExtension(GUISelection.fileName);

            ParsingMessageLabel.Text = "Analyzing...";

            manager = new GamingSupervisorManager();

            worker = new BackgroundWorker();
            worker.DoWork += RunGamingSupervisor;
            worker.RunWorkerCompleted += Finished;

            worker.RunWorkerAsync();
        }

        private void RunGamingSupervisor(object sender, DoWorkEventArgs e)
        {
            manager.Start();
        }

        private void Finished(object sender, RunWorkerCompletedEventArgs e)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            DifficultySelection difficultySelection = new DifficultySelection();
            navService.Navigate(difficultySelection);
        }
    }
}
