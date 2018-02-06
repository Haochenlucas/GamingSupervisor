namespace GamingSupervisor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            DifficultySelection difficultySelection = new DifficultySelection();
            MainFrame.Navigate(difficultySelection);
        }
    }
}
