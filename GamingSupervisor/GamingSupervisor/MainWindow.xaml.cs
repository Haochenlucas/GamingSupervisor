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

            GUISelection selection = new GUISelection();
            DifficultySelection difficultySelection = new DifficultySelection(selection);
            MainFrame.Navigate(difficultySelection);
        }
    }
}
