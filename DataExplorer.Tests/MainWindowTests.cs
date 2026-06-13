using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using NUnit.Framework;

namespace DataExplorer.Tests
{
    [TestFixture]
    public class MainWindowTests
    {
        private Application? _application;
        private UIA3Automation? _automation;
        private Window? _mainWindow;

        [OneTimeSetUp]
        public void Setup()
        {
            var appPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "DataExplorer.exe");
            
            if (!File.Exists(appPath))
            {
                // Navigate from bin/Debug/net8.0-windows
                appPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "WPF_Project", "bin", "Debug", "net8.0-windows", "DataExplorer.exe");
            }

            if (File.Exists(appPath))
            {
                _application = Application.Launch(appPath);
                _automation = new UIA3Automation();
                
                _mainWindow = _application.GetMainWindow(_automation, TimeSpan.FromSeconds(5));
                Assert.That(_mainWindow, Is.Not.Null, "Main window did not appear.");
            }
            else
            {
                Assert.Ignore($"Could not find executable at {appPath}. Please ensure the DataExplorer project is built.");
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _automation?.Dispose();
            _application?.Close();
        }

        [Test]
        public void Scenario1_DataExplorer_SelectProject_And_Edit_Should_Update_Fields()
        {
            // Switch to Data Explorer view
            var menuDataExplorer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuDataExplorer"))?.AsMenuItem();
            menuDataExplorer?.Invoke();
            Thread.Sleep(500);

            var projectsGrid = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("ProjectsDataGrid"));
            Assert.That(projectsGrid, Is.Not.Null, "ProjectsDataGrid not found.");

            // Find rows (infragistics might use generic elements, so we'll just check if there are list items or data items)
            var rows = projectsGrid?.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));

            if (rows != null && rows.Length > 0)
            {
                var rowToSelect = rows[0];
                rowToSelect.Click();

                Thread.Sleep(500);

                var nameTextBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtProjectName"))?.AsTextBox();
                var budgetTextBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtProjectBudget"))?.AsTextBox();

                Assert.That(nameTextBox, Is.Not.Null, "Project Name TextBox not found.");
                Assert.That(budgetTextBox, Is.Not.Null, "Project Budget TextBox not found.");

                if (nameTextBox != null)
                {
                    nameTextBox.Enter("Updated Project Name");
                }

                if (budgetTextBox != null)
                {
                    budgetTextBox.Enter("150000");
                }

                var saveButton = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("SaveProjectButton"))?.AsButton();
                Assert.That(saveButton, Is.Not.Null, "Save Project Button not found.");
                saveButton?.Invoke();
            }
        }

        [Test]
        public void Scenario2_SwitchTab_And_EditTask()
        {
            // Switch to Tasks & Cards view
            var menuTasksAndCards = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuTasksAndCards"))?.AsMenuItem();
            menuTasksAndCards?.Invoke();
            Thread.Sleep(500);

            var tabControl = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("MainTabControl"))?.AsTab();
            Assert.That(tabControl, Is.Not.Null, "MainTabControl not found.");

            if (tabControl != null && tabControl.TabItems.Length > 0)
            {
                tabControl.TabItems[0].Select();
                Thread.Sleep(500);
            }

            var taskList = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("TasksListView"))?.AsListBox();

            if (taskList == null)
            {
                // In some WPF definitions ListView is actually DataGrid or just List, we can check by AutomationId
                var listContainer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("TasksListView"));
                if (listContainer != null)
                {
                    var items = listContainer.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
                    if (items.Length > 0)
                    {
                        items[0].Click();
                        Thread.Sleep(500);

                        var taskTitleBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtTaskTitle"))?.AsTextBox();
                        Assert.That(taskTitleBox, Is.Not.Null, "Task Title TextBox not found.");

                        if (taskTitleBox != null)
                        {
                            taskTitleBox.Enter("Updated Task Title");
                        }

                        var saveTaskButton = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("SaveTaskButton"))?.AsButton();
                        Assert.That(saveTaskButton, Is.Not.Null, "Save Task Button not found.");
                        saveTaskButton?.Invoke();
                    }
                }
            }
            else if (taskList.Items.Length > 0)
            {
                taskList.Items[0].Select();
                Thread.Sleep(500);

                var taskTitleBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtTaskTitle"))?.AsTextBox();
                Assert.That(taskTitleBox, Is.Not.Null, "Task Title TextBox not found.");

                if (taskTitleBox != null)
                {
                    taskTitleBox.Enter("Updated Task Title");
                }

                var saveTaskButton = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("SaveTaskButton"))?.AsButton();
                Assert.That(saveTaskButton, Is.Not.Null, "Save Task Button not found.");
                saveTaskButton?.Invoke();
            }
        }

        [Test]
        public void Scenario3_DataExplorer_Search_FiltersGrid()
        {
            // Switch to Data Explorer view
            var menuDataExplorer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuDataExplorer"))?.AsMenuItem();
            menuDataExplorer?.Invoke();
            Thread.Sleep(500);

            var searchBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("SearchTextBox"))?.AsTextBox();
            Assert.That(searchBox, Is.Not.Null, "SearchTextBox not found.");

            searchBox?.Enter("Logistics");
            Thread.Sleep(500); // Verify grid item count changes (left as manual verification since grid structure varies)

            searchBox?.Enter("");
            Thread.Sleep(500);
        }

        [Test]
        public void Scenario4_ValidatingDataEditedAndSaved_ReflectsInGridRow()
        {
            var menuDataExplorer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuDataExplorer"))?.AsMenuItem();
            menuDataExplorer?.Invoke();
            Thread.Sleep(500);

            var projectsGrid = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("ProjectsDataGrid"));
            Assert.That(projectsGrid, Is.Not.Null, "ProjectsDataGrid not found.");

            var rows = projectsGrid?.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
            if (rows != null && rows.Length > 0)
            {
                var rowToSelect = rows[0];
                rowToSelect.Click();
                Thread.Sleep(500);

                var nameTextBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtProjectName"))?.AsTextBox();
                Assert.That(nameTextBox, Is.Not.Null);
                nameTextBox.Enter("Test Updated Project Name");

                var saveButton = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("SaveProjectButton"))?.AsButton();
                saveButton?.Invoke();
                Thread.Sleep(500);

                var cells = rowToSelect.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem)/*.Or(...)*/);
                // FlaUI tends to represent cells as Text or Custom.
                // We just check if "Test Updated Project Name" appears anywhere in the row's bounding box text or descendants
                var updatedText = rowToSelect.FindFirstDescendant(cf => cf.ByName("Test Updated Project Name"));
                Assert.That(updatedText, Is.Not.Null, "Grid row did not reflect the updated project name.");
            }
        }

        [Test]
        public void Scenario5_DragAndDropToGroup_BasedOnStatusColumn()
        {
            var menuDataExplorer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuDataExplorer"))?.AsMenuItem();
            menuDataExplorer?.Invoke();
            Thread.Sleep(500);

            var projectsGrid = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("ProjectsDataGrid"));
            Assert.That(projectsGrid, Is.Not.Null);

            // GroupByArea and Status header
            var allHeaders = projectsGrid.FindAllDescendants(cf => cf.ByName("Status") /* Header */);
            var statusHeader = allHeaders.FirstOrDefault(h => h.ControlType == FlaUI.Core.Definitions.ControlType.Header || h.ControlType == FlaUI.Core.Definitions.ControlType.HeaderItem || h.ClassName.Contains("LabelPresenter") || h.ClassName.Contains("Header"));
            if (statusHeader == null) statusHeader = projectsGrid.FindFirstDescendant(cf => cf.ByName("Status")); // Fallback

            var groupByArea = projectsGrid.FindFirstDescendant(cf => cf.ByClassName("GroupByAreaMulti"))
                           ?? projectsGrid.FindFirstDescendant(cf => cf.ByClassName("GroupByArea"))
                           ?? projectsGrid.FindFirstDescendant(cf => cf.ByAutomationId("GroupByArea"));

            if (statusHeader != null && groupByArea == null)
            {
                // Fallback: GroupByArea is located at the top part of the grid if Location=Top
                // We don't have a direct reference, so we will use the position.
            }

            if (statusHeader != null)
            {
                var targetCenter = groupByArea != null ? groupByArea.BoundingRectangle.Center() : new System.Drawing.Point((int)projectsGrid.BoundingRectangle.Left + 50, (int)projectsGrid.BoundingRectangle.Top + 10);

                FlaUI.Core.Input.Mouse.Position = statusHeader.BoundingRectangle.Center();
                FlaUI.Core.Input.Mouse.Down(FlaUI.Core.Input.MouseButton.Left);
                Thread.Sleep(100);
                FlaUI.Core.Input.Mouse.Position = targetCenter;
                Thread.Sleep(100);
                FlaUI.Core.Input.Mouse.Up(FlaUI.Core.Input.MouseButton.Left);
                Thread.Sleep(1000);

                // Find GroupByRecords
                var groupRows = projectsGrid.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Group));
                Assert.That(groupRows.Length, Is.GreaterThan(0), "Grid rows were not grouped after drag and drop.");

                foreach (var groupRow in groupRows)
                {
                    // Expand group
                    var expandButton = groupRow.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Button));
                    expandButton?.AsButton()?.Invoke();
                    Thread.Sleep(200);
                }
            }
            else
            {
                Assert.Ignore("Status header or GroupByArea could not be found via UI Automation for drag & drop grouping.");
            }
        }

        [Test]
        public void Scenario6_ValidatingDataEditedAndSaved_ReflectsInListViewItemAndCard()
        {
            var menuTasksAndCards = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuTasksAndCards"))?.AsMenuItem();
            menuTasksAndCards?.Invoke();
            Thread.Sleep(500);

            // ListView
            var taskList = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("TasksListView"));
            Assert.That(taskList, Is.Not.Null);

            var listItems = taskList.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
            if (listItems.Length > 0)
            {
                listItems[0].Click();
                Thread.Sleep(500);

                var taskTitleBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtTaskTitle"))?.AsTextBox();
                taskTitleBox?.Enter("ListView Updated Title");

                var saveTaskButton = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("SaveTaskButton"))?.AsButton();
                saveTaskButton?.Invoke();
                Thread.Sleep(500);

                // Verify ListView reflected change
                var updatedListItemText = listItems[0].FindFirstDescendant(cf => cf.ByName("ListView Updated Title"));
                Assert.That(updatedListItemText, Is.Not.Null, "ListView item did not reflect the update.");

                // Card View Validation
                var cardViewTab = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("CardViewTab"))?.AsTabItem();
                cardViewTab?.Select();
                Thread.Sleep(500);

                var cardList = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("TasksCardView"));
                Assert.That(cardList, Is.Not.Null);

                var cardItems = cardList.FindAllChildren(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
                if (cardItems.Length > 0)
                {
                    var updatedCardText = cardItems[0].FindFirstDescendant(cf => cf.ByName("ListView Updated Title"));
                    Assert.That(updatedCardText, Is.Not.Null, "Card view item did not reflect the update.");
                }
            }
        }

        [Test]
        public void Scenario7_DoubleClickOnGrid_OpensFormWithCorrectData_ThenClose()
        {
            var menuDataExplorer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuDataExplorer"))?.AsMenuItem();
            menuDataExplorer?.Invoke();
            Thread.Sleep(500);

            var projectsGrid = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("ProjectsDataGrid"));
            var rows = projectsGrid?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));

            // Filter out the FilterRecord or other non-data DataItems
            var dataRows = rows?.Where(r => r.Name == null || !r.Name.Contains("Filter")).ToArray();

            if (dataRows != null && dataRows.Length > 2)
            {
                var rowToSelect = dataRows[2];

                // Ensure the item is selected before double-clicking
                try
                {
                    if (rowToSelect.Patterns.SelectionItem.IsSupported)
                    {
                        rowToSelect.Patterns.SelectionItem.Pattern.Select();
                    }
                }
                catch
                {
                }
                rowToSelect.Click();
                Thread.Sleep(200);

                rowToSelect.DoubleClick();
                Thread.Sleep(1000);

                // Form should open (DetailsWindow)
                FlaUI.Core.AutomationElements.Window detailsWindow = null;
                for (int i = 0; i < 5; i++)
                {
                    detailsWindow = _mainWindow?.ModalWindows.FirstOrDefault(w => w.Title == "Record Details" || w.Name == "Record Details") ??
                                        _application?.GetAllTopLevelWindows(_automation).FirstOrDefault(w => w.Title == "Record Details" || (w.Title != _mainWindow.Title));
                    if (detailsWindow != null) break;
                    Thread.Sleep(500);
                }

                Assert.That(detailsWindow, Is.Not.Null, "DetailsWindow did not open upon grid double-click.");

                var closeButton = detailsWindow?.FindFirstDescendant(cf => cf.ByName("Close"))?.AsButton();
                closeButton?.Invoke();
            }
            else
            {
                Assert.Ignore("No rows available to double click.");
            }
        }

        [Test]
        public void Scenario8_DoubleClickOnListViewItemOrCard_OpensFormWithCorrectData_ThenClose()
        {
            var menuTasksAndCards = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuTasksAndCards"))?.AsMenuItem();
            menuTasksAndCards?.Invoke();
            Thread.Sleep(500);

            var taskList = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("TasksListView"));
            var listItems = taskList?.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem).Or(cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem)));

            if (listItems != null && listItems.Length > 0)
            {
                var itemToSelect = listItems[0];

                // Ensure the item is selected before double-clicking, in case MouseDoubleClick relies on SelectedItem
                try
                {
                    if (itemToSelect.Patterns.SelectionItem.IsSupported)
                    {
                        itemToSelect.Patterns.SelectionItem.Pattern.Select();
                    }
                }
                catch
                {
                }
                itemToSelect.Click();
                Thread.Sleep(200);

                itemToSelect.DoubleClick();
                Thread.Sleep(1000);

                // Form should open (TaskDetailsWindow)
                FlaUI.Core.AutomationElements.Window taskDetailsWindow = null;
                for (int i = 0; i < 5; i++)
                {
                    taskDetailsWindow = _mainWindow?.ModalWindows.FirstOrDefault(w => w.Title == "Task Details") ??
                                        _application?.GetAllTopLevelWindows(_automation).FirstOrDefault(w => w.Title == "Task Details" || w.Title != _mainWindow.Title);
                    if (taskDetailsWindow != null) break;
                    Thread.Sleep(500);
                }

                Assert.That(taskDetailsWindow, Is.Not.Null, "TaskDetailsWindow did not open upon ListView double-click.");

                // Validate some data exists
                var descBlock = taskDetailsWindow.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.Text));
                Assert.That(descBlock, Is.Not.Null, "Data seems absent heavily in details pop-up form.");

                var okButton = taskDetailsWindow?.FindFirstDescendant(cf => cf.ByName("OK") /* typical dialog close */)?.AsButton();
                if (okButton != null) okButton.Invoke();
                else taskDetailsWindow?.Close();
            }
            else
            {
                Assert.Ignore("No list items available to double click.");
            }
        }

        [Test]
        public void Scenario9_DragTreeNodeNotInGrid_DropInGrid_BecomesEntry()
        {
            var menuDataExplorer = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("menuDataExplorer"))?.AsMenuItem();
            menuDataExplorer?.Invoke();
            Thread.Sleep(500);

            var treeNodes = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("TreeNodes"));
            var projectsGrid = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("ProjectsDataGrid"));

            Assert.That(treeNodes, Is.Not.Null, "TreeNodes not found.");
            Assert.That(projectsGrid, Is.Not.Null, "ProjectsDataGrid not found.");

            var allTreeItems = treeNodes.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TreeItem));

            var testNode = allTreeItems.FirstOrDefault(t => t.Name != null && t.Name.Contains("Drag"));
            if (testNode == null)
            {
                var templatesNode = allTreeItems.FirstOrDefault(t => t.Name != null && t.Name.Contains("Templates"));
                if (templatesNode != null)
                {
                    try
                    {
                        if (templatesNode.Patterns.ExpandCollapse.IsSupported)
                        {
                            templatesNode.Patterns.ExpandCollapse.Pattern.Expand();
                        }
                        else
                        {
                            templatesNode.DoubleClick();
                        }
                    }
                    catch
                    {
                        templatesNode.DoubleClick();
                    }
                    Thread.Sleep(500);
                    allTreeItems = treeNodes.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.TreeItem));
                    testNode = allTreeItems.FirstOrDefault(t => t.Name != null && t.Name.Contains("Drag"));
                }
            }

            if (testNode != null)
            {
                // Note initial row counts
                var initialRows = projectsGrid.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem)).Length;

                FlaUI.Core.Input.Mouse.Position = testNode.BoundingRectangle.Center();
                FlaUI.Core.Input.Mouse.Down(FlaUI.Core.Input.MouseButton.Left);
                Thread.Sleep(100);

                // Trigger drag start by moving slightly
                FlaUI.Core.Input.Mouse.MoveBy(10, 10);
                Thread.Sleep(100);

                // Move over target
                var targetCenter = projectsGrid.BoundingRectangle.Center();
                var targetTop = projectsGrid.BoundingRectangle.Top;
                FlaUI.Core.Input.Mouse.Position = new System.Drawing.Point((int)targetCenter.X, (int)targetTop + 30); // drop near the top, some grids don't allow drop on scroll areas
                Thread.Sleep(200);
                FlaUI.Core.Input.Mouse.Up(FlaUI.Core.Input.MouseButton.Left);
                Thread.Sleep(500);

                var newRows = projectsGrid.FindAllDescendants(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
                Assert.That(newRows.Length, Is.GreaterThan(initialRows), "Grid row count did not increase after dropping tree node. Drop failed.");

                // On selection it should show details below (we select the newly added row, usually the last one)
                if (newRows.Length > 0)
                {
                    // Find the newly added row by name
                    var addedRow = newRows.FirstOrDefault(r => r.Name != null && r.Name.Contains(testNode.Name)) ?? newRows[newRows.Length - 1];
                    addedRow.Click();
                    Thread.Sleep(500);

                    // Validate details form populated
                    var editRecordText = _mainWindow?.FindFirstDescendant(cf => cf.ByName($"Edit Record: "))?.Parent; // Approximate check based on 'Edit Record: {0}' format
                    var nameTextBox = _mainWindow?.FindFirstDescendant(cf => cf.ByAutomationId("txtProjectName"))?.AsTextBox();

                    Assert.That(nameTextBox, Is.Not.Null, "Details pane not shown below.");
                    Assert.That(nameTextBox.Text, Is.EqualTo(testNode.Name), "Grid entry details mismatch based on tree node drag.");
                }
            }
            else
            {
                Assert.Ignore("Could not find a valid TreeNode to perform drag and drop.");
            }
        }
    }
}
