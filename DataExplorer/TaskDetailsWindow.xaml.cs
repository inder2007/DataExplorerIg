using System.Windows;

namespace DataExplorer
{
    public partial class TaskDetailsWindow : Window
    {
        public TaskDetailsWindow(TaskItem task)
        {
            InitializeComponent();
            DataContext = task;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
