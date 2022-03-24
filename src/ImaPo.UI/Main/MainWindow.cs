// Copyright (c) 2022 Benito Palacios Sánchez

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
        string versionText = version.Build == 0 ? "DEVELOPMENT BUILD" : $"v{version}";
        Title = "ImaPo ~~ " + versionText;
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
                new Command {
                    MenuText = "&Upload screenshots",
                    Shortcut = Application.Instance.CommonModifier | Keys.U,
                    DelegatedCommand = viewModel.UploadScreenshotCommand,
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
        var scrollableImage = new Scrollable { Content = imageView };

        var contextBox = new TextBox { ReadOnly = true };
        _ = contextBox.TextBinding.BindDataContext((MainViewModel vm) => vm.ContextText);

        imageTextBox = new TextArea();
        _ = imageTextBox.TextBinding.BindDataContext((MainViewModel vm) => vm.ImageText);

        var poTable = new TableLayout {
            Padding = new Padding(5),
            Spacing = new Size(5, 5),
            Rows = {
                new TableRow(
                    new Label { Text = "Context", VerticalAlignment = VerticalAlignment.Center },
                    new TableCell(contextBox, true)),
                new TableRow(
                    new Label { Text = "Text", VerticalAlignment = VerticalAlignment.Top },
                    new TableCell(imageTextBox, true)),
            },
        };

        var savingLabel = new Label {
            Text = "Saves synchronously on typing",
            Font = new Font("Ubuntu Nerd Font", 10, FontStyle.Italic),
        };

        var contentPanel = new StackLayout(scrollableImage, null, savingLabel, poTable) {
            Padding = new Padding(5),
            Spacing = 5,
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

        return new Splitter {
            Orientation = Orientation.Horizontal,
            FixedPanel = SplitterFixedPanel.Panel1,
            Panel1MinimumSize = 300,
            Panel1 = CreateTreeBar(),
            Panel2 = contentPanel,
        };
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

        return new Scrollable { Content = tree };
    }
}
