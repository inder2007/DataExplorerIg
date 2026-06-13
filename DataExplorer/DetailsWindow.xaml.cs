using System.Windows;

namespace DataExplorer
{
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(Project project)
        {
            InitializeComponent();
            DataContext = project;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
