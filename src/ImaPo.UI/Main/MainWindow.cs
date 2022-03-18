using System;
using System.Reflection;
using Eto.Drawing;
using Eto.Forms;

namespace ImaPo.UI.Main;

public sealed class MainWindow : Form
{
    private readonly MainViewModel viewModel;
    private TextArea imageTextBox;
    private TreeGridView tree;

    public MainWindow()
    {
        viewModel = new MainViewModel();
        DataContext = viewModel;

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Icon = Icon.FromResource(ResourcesName.Icon)
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        Title = $"ImaPo ~~ {version}";
        ClientSize = new Size(800, 600);

        Menu = CreateMenuBar();
        Content = CreateContent();
    }

    private MenuBar CreateMenuBar() =>
        new MenuBar {
            Items = { new ButtonMenuItem { Text = "&File", }, },
            ApplicationItems = {
                new Command {
                    MenuText = "&New project",
                    Shortcut = Application.Instance.CommonModifier | Keys.O,
                    DelegatedCommand = viewModel.NewProjectCommand,
                },
                new Command {
                    MenuText = "&Open project",
                    Shortcut = Application.Instance.CommonModifier | Keys.O,
                    DelegatedCommand = viewModel.OpenProjectCommand,
                },
            },
            QuitItem = new Command {
                MenuText = "&Quit",
                Shortcut = Application.Instance.CommonModifier | Keys.Q,
                DelegatedCommand = viewModel.QuitCommand,
            },
            AboutItem = new Command { MenuText = "About...", DelegatedCommand = viewModel.AboutCommand, },
        };

    private Control CreateContent()
    {
        var imageView = new ImageView();
        _ = imageView.BindDataContext(v => v.Image, (MainViewModel vm) => vm.CurrentImage);

        var contextBox = new TextBox { ReadOnly = true };
        _ = contextBox.TextBinding.BindDataContext((MainViewModel vm) => vm.ContextText);

        imageTextBox = new TextArea();
        _ = imageTextBox.TextBinding.BindDataContext((MainViewModel vm) => vm.ImageText);
        imageTextBox.KeyUp += (_, e) => {
            if (e.Control && e.Key == Keys.Down) {
                Binding.ExecuteCommand(
                    viewModel,
                    Binding.Property((MainViewModel vm) => vm.SelectNextNodeCommand));
            }
        };

        var statusLabel = new Label { Text = "Saved!" };
        var poTable = new TableLayout {
            Padding = new Padding(10),
            Spacing = new Size(5, 5),
            Rows = {
                new TableRow(
                    new Label { Text = "Context", VerticalAlignment = VerticalAlignment.Center },
                    new TableCell(contextBox, true)),
                new TableRow(
                    new Label { Text = "Text", VerticalAlignment = VerticalAlignment.Top },
                    new TableCell(imageTextBox, true),
                    statusLabel),
            },
        };

        var contentPanel = new StackLayout(imageView, null, poTable) {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

        var splitter = new Splitter {
            Orientation = Orientation.Horizontal,
            FixedPanel = SplitterFixedPanel.Panel1,
            Panel1MinimumSize = 300,
            Panel1 = CreateTreeBar(),
            Panel2 = contentPanel,
        };

        return splitter;
    }

    private Panel CreateTreeBar()
    {
        // Tree-view with just one "column" with icon and name, so the icon is close to the name.
        tree = new TreeGridView { ShowHeader = false, Border = BorderType.Line, Style = "analyze-tree", };
        tree.Columns.Add(
            new GridColumn {
                DataCell = new TextBoxCell { Binding = Binding.Property((TreeGridNode node) => node.QualifiedName) },
                HeaderText = "name",
                AutoSize = true,
                Resizable = false,
                Editable = false,
            });
        tree.DataStore = viewModel.RootNode;
        _ = tree.SelectedItemBinding.BindDataContext((MainViewModel vm) => vm.SelectedNode);
        tree.SelectedItemChanged += (_, _) => Binding.ExecuteCommand(
            viewModel,
            Binding.Property((MainViewModel vm) => vm.OpenImageCommand));
        tree.SelectedItemChanged += (_, _) => imageTextBox.SelectAll();

        // Eto doesn't implement the binding fully: https://github.com/picoe/Eto/issues/240
        viewModel.OnNodeUpdate += (_, node) => {
            if (node is null) {
                tree.DataStore = viewModel.RootNode;
                tree.ReloadData();
            } else {
                tree.ReloadItem(node);
            }
        };

        var scrollableTree = new Scrollable { Content = tree };
        return scrollableTree;
    }
}
