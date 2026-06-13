using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using Newtonsoft.Json;

namespace DataExplorer
{
    // --- Relay Command ---
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        public RelayCommand(Action<object> execute) { this.execute = execute; }
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => execute(parameter);
    }

    // --- Base Model ---
    public class BaseModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // --- Data Models ---
    public class Project : BaseModel
    {
        public string Id { get; set; }
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        private double _budget;
        public double Budget { get => _budget; set { _budget = value; OnPropertyChanged(); } }
        private string _status;
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }
        private string _details;
        public string Details { get => _details; set { _details = value; OnPropertyChanged(); } }
        private string _category;
        public string Category { get => _category; set { _category = value; OnPropertyChanged(); } }
        private bool _isEditable;
        public bool IsEditable { get => _isEditable; set { _isEditable = value; OnPropertyChanged(); } }
    }

    public class TaskItem : BaseModel
    {
        public string Id { get; set; }
        private string _title;
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        private string _assignee;
        public string Assignee { get => _assignee; set { _assignee = value; OnPropertyChanged(); } }
        private string _priority;
        public string Priority { get => _priority; set { _priority = value; OnPropertyChanged(); } }
        private string _date;
        public string Date { get => _date; set { _date = value; OnPropertyChanged(); } }
    }

    public class TreeNode
    {
        public string Name { get; set; }
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
    }

    public class AppData
    {
        public List<TreeNode> TreeNodes { get; set; }
        public List<Project> Projects { get; set; }
        public List<TaskItem> Tasks { get; set; }
    }

    // --- Main ViewModel ---
    public class MainViewModel : BaseModel
    {
        private string _currentViewName = "Grid";
        public string CurrentViewName 
        { 
            get => _currentViewName; 
            set { _currentViewName = value; OnPropertyChanged(); } 
        }

        public ObservableCollection<TreeNode> TreeNodes { get; set; }
        public ObservableCollection<Project> Projects { get; set; }
        public ObservableCollection<TaskItem> Tasks { get; set; }

        public ICollectionView ProjectsView { get; set; }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set 
            { 
                _selectedProject = value; 
                if (value != null)
                {
                    DraftProject = new Project
                    {
                        Id = value.Id, Name = value.Name, Budget = value.Budget, Status = value.Status, Details = value.Details, Category = value.Category, IsEditable = value.IsEditable
                    };
                }
                else
                {
                    DraftProject = null;
                }
                OnPropertyChanged(); 
            }
        }

        private Project _draftProject;
        public Project DraftProject
        {
            get => _draftProject;
            set { _draftProject = value; OnPropertyChanged(); }
        }

        private TaskItem _selectedTask;
        public TaskItem SelectedTask
        {
            get => _selectedTask;
            set 
            { 
                _selectedTask = value; 
                if (value != null)
                {
                    DraftTask = new TaskItem
                    {
                        Id = value.Id, Title = value.Title, Description = value.Description, Assignee = value.Assignee, Priority = value.Priority, Date = value.Date
                    };
                }
                else
                {
                    DraftTask = null;
                }
                OnPropertyChanged(); 
            }
        }

        private TaskItem _draftTask;
        public TaskItem DraftTask
        {
            get => _draftTask;
            set { _draftTask = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set 
            { 
                _searchText = value; 
                OnPropertyChanged(); 
                ProjectsView?.Refresh(); 
            }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set 
            { 
                _selectedCategory = value; 
                OnPropertyChanged(); 
                ProjectsView?.Refresh(); 
            }
        }

        public ICommand ShowGridCommand { get; }
        public ICommand ShowListCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand CancelProjectCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand CancelTaskCommand { get; }

        public MainViewModel()
        {
            ShowGridCommand = new RelayCommand(o => CurrentViewName = "Grid");
            ShowListCommand = new RelayCommand(o => CurrentViewName = "List");

            SaveProjectCommand = new RelayCommand(o => {
                if (SelectedProject != null && DraftProject != null) {
                    SelectedProject.Name = DraftProject.Name;
                    SelectedProject.Budget = DraftProject.Budget;
                    SelectedProject.Status = DraftProject.Status;
                    SelectedProject.Details = DraftProject.Details;
                    SelectedProject.Category = DraftProject.Category;
                    ProjectsView?.Refresh();
                }
            });

            CancelProjectCommand = new RelayCommand(o => {
                if (SelectedProject != null) {
                    DraftProject = new Project
                    {
                        Id = SelectedProject.Id, Name = SelectedProject.Name, Budget = SelectedProject.Budget, Status = SelectedProject.Status, Details = SelectedProject.Details, Category = SelectedProject.Category, IsEditable = SelectedProject.IsEditable
                    };
                }
            });

            SaveTaskCommand = new RelayCommand(o => {
                if (SelectedTask != null && DraftTask != null) {
                    SelectedTask.Title = DraftTask.Title;
                    SelectedTask.Description = DraftTask.Description;
                    SelectedTask.Assignee = DraftTask.Assignee;
                    SelectedTask.Priority = DraftTask.Priority;
                    SelectedTask.Date = DraftTask.Date;
                }
            });

            CancelTaskCommand = new RelayCommand(o => {
                if (SelectedTask != null) {
                    DraftTask = new TaskItem
                    {
                        Id = SelectedTask.Id, Title = SelectedTask.Title, Description = SelectedTask.Description, Assignee = SelectedTask.Assignee, Priority = SelectedTask.Priority, Date = SelectedTask.Date
                    };
                }
            });

            LoadData();

            ProjectsView = CollectionViewSource.GetDefaultView(Projects);
            ProjectsView.Filter = FilterProjects;
        }

        private bool FilterProjects(object item)
        {
            if (item is Project p)
            {
                bool matchesSearch = string.IsNullOrWhiteSpace(SearchText) ||
                       p.Name.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                       p.Details.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
                
                return matchesSearch;
            }
            return false;
        }

        private void LoadData()
        {
            try
            {
                string json = File.ReadAllText("data.json");
                var data = JsonConvert.DeserializeObject<AppData>(json);
                
                TreeNodes = new ObservableCollection<TreeNode>(data.TreeNodes);
                Projects = new ObservableCollection<Project>(data.Projects);
                Tasks = new ObservableCollection<TaskItem>(data.Tasks);
            }
            catch
            {
                // Fallback static data if file read fails during preview
                TreeNodes = new ObservableCollection<TreeNode>
                {
                    new TreeNode { Name = "Projects", Children = new List<TreeNode> { new TreeNode { Name = "Default" } } }
                };
                Projects = new ObservableCollection<Project>
                {
                    new Project { Id = "1", Name = "Sample Project", Budget = 1000, Status = "Active", Details = "File not found fallback." }
                };
                Tasks = new ObservableCollection<TaskItem>
                {
                    new TaskItem { Id = "T1", Title = "Config", Description = "Configure JSON path.", Assignee = "Dev", Priority = "Medium" }
                };
            }
        }
    }
}
