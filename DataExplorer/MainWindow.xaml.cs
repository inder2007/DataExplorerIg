using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.Windows.DataPresenter.Events;

namespace DataExplorer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TreeView_ActiveNodeChanged(object sender, Infragistics.Controls.Menus.ActiveNodeChangedEventArgs e)
        {
            if (e.NewActiveTreeNode?.Data is TreeNode selectedNode && DataContext is MainViewModel vm)
            {
                vm.SelectedCategory = selectedNode.Name;
                
                var existingItem = vm.Projects.FirstOrDefault(p => p.Name == selectedNode.Name);
                if (existingItem != null)
                {
                    vm.SelectedProject = existingItem;
                }
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Infragistics.Windows.DataPresenter.XamDataGrid grid && grid.ActiveDataItem is Project selectedProject)
            {
                var detailsWindow = new DetailsWindow(selectedProject);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
        }

        private void Tasks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is TaskItem selectedTask)
            {
                var detailsWindow = new TaskDetailsWindow(selectedTask);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
            else if (sender is ListView listView && listView.SelectedItem is TaskItem listViewTask)
            {
                var detailsWindow = new TaskDetailsWindow(listViewTask);
                detailsWindow.Owner = this;
                detailsWindow.ShowDialog();
            }
        }
        private Point _dragStartPoint;

        private void TreeNodes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }

        private void TreeNodes_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point mousePos = e.GetPosition(null);
            Vector diff = _dragStartPoint - mousePos;

            if (System.Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || 
                System.Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                var fe = e.OriginalSource as FrameworkElement;
                var dtc = fe?.DataContext;
                TreeNode treeNode = null;

                if (dtc is Infragistics.Controls.Menus.XamDataTreeNode igNode)
                {
                    treeNode = igNode.Data as TreeNode;
                }
                else if (dtc is TreeNode tn)
                {
                    treeNode = tn;
                }
                else if (sender is Infragistics.Controls.Menus.XamDataTree tree)
                {
                    treeNode = tree.ActiveNode?.Data as TreeNode;
                }

                if (treeNode != null)
                {
                    DragDrop.DoDragDrop(sender as DependencyObject, treeNode, DragDropEffects.Copy);
                }
            }
        }

        private void ProjectsDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                var node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
                if (node != null && DataContext is MainViewModel vm)
                {
                    if (vm.Projects.Any(p => p.Name == node.Name))
                    {
                        MessageBox.Show("This record is already in the list.", "Duplicate Record", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var newProject = new Project
                        {
                            Id = "D" + new System.Random().Next(100, 999),
                            Name = node.Name,
                            Budget = 50000,
                            Status = "Pending",
                            Details = "Created from Drag operation",
                            Category = "R&D",
                            IsEditable = true
                        };
                        vm.Projects.Add(newProject);
                        vm.SelectedProject = newProject;
                    }
                }
                e.Handled = true;
            }
        }

        private void ProjectsDataGrid_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ProjectsDataGrid_EditModeStarting(object sender, EditModeStartingEventArgs e)
        {
            if (e.Cell.Record.DataItem is Project project && !project.IsEditable)
            {
                e.Cancel = true;
            }
        }
    }
}
