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
using System.IO;
using System.Windows.Input;
using Eto.Drawing;
using Eto.Forms;
using ImaPo.UI.Projects;
using ImaPo.UI.ScreenshotUpload;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using YamlDotNet.Serialization;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace ImaPo.UI.Main;

public sealed class MainViewModel : ObservableObject
{
    private ProjectSettings? project;
    private ProjectManager projectManager;

    private TreeGridNode? openedNode;
    private TreeGridNode selectedNode;
    private Image currentImage;
    private string contextText;
    private string imageText;

    public MainViewModel()
    {
        QuitCommand = new RelayCommand(Quit);
        AboutCommand = new RelayCommand(OpenAboutDialog);
        OpenProjectCommand = new RelayCommand(OpenProject);
        NewProjectCommand = new RelayCommand(NewProject);
        OpenImageCommand = new RelayCommand(OpenImage);
        SelectNextNodeCommand = new RelayCommand(SelectNextNode);
        UploadScreenshotCommand = new RelayCommand(OpenUploadScreenshot, () => project is not null);

        var dummyProject = new ProjectManager(new ProjectSettings());
        RootNode = new TreeGridNode(NodeFactory.CreateContainer("root"), dummyProject);
    }

    public event EventHandler<TreeGridNode?> OnNodeUpdate;

    public ICommand QuitCommand { get; }

    public ICommand AboutCommand { get; }

    public ICommand NewProjectCommand { get; }

    public ICommand OpenProjectCommand { get; }

    public ICommand OpenImageCommand { get; }

    public ICommand SelectNextNodeCommand { get; }

    public RelayCommand UploadScreenshotCommand { get; }

    public TreeGridNode? SelectedNode {
        get => selectedNode;
        set => SetProperty(ref selectedNode, value);
    }

    public TreeGridNode RootNode { get; set; }

    public Image CurrentImage {
        get => currentImage;
        set => SetProperty(ref currentImage, value);
    }

    public string ContextText {
        get => contextText;
        set => SetProperty(ref contextText, value);
    }

    public string ImageText {
        get => imageText;
        set {
            if (SetProperty(ref imageText, value)) {
                OnImageTextChange();
            }
        }
    }

    public void NewProject()
    {
        _ = MessageBox.Show("Not supported yet, sorry. Create the project by hand.");
    }

    public void OpenProject()
    {
        var dialog = new OpenFileDialog {
            Title = "Open project file",
            Filters = { new FileFilter("imapo.yml", ".yml", ".yaml") },
        };
        if (dialog.ShowDialog(Application.Instance.MainForm) != DialogResult.Ok) {
            return;
        }

        try {
            string projectText = File.ReadAllText(dialog.FileName);
            project = new DeserializerBuilder()
                .Build()
                .Deserialize<ProjectSettings>(projectText);
            projectManager = new ProjectManager(project);

            string projectPath = Path.GetDirectoryName(dialog.FileName) ?? throw new FileNotFoundException("Invalid path");
            string imagePath = Path.Combine(projectPath, project.ImageFolder);
            string textPath = Path.Combine(projectPath, project.TextFolder);
            if (!Directory.Exists(textPath)) {
                _ = Directory.CreateDirectory(textPath);
            }

            RootNode.Node.Dispose();
            RootNode = new TreeGridNode(NodeFactory.CreateContainer("root"), projectManager);
            RootNode.Add(NodeFactory.FromDirectory(imagePath, "*", "Images", subDirectories: true, FileOpenMode.Read));
        } catch (Exception ex) {
            _ = MessageBox.Show($"Error opening project file: {ex}", MessageBoxType.Error);
            return;
        }

#pragma warning disable S4220 // pending good refactor
        OnNodeUpdate?.Invoke(this, null);
#pragma warning restore S4220

        UploadScreenshotCommand.NotifyCanExecuteChanged();
    }

    public void SelectNextNode()
    {
        TreeGridNode? selected = SelectedNode;
        if (selected is null) {
            return;
        }

        Node current = selected.Node;
        Node parent = current.Parent;

        int currentIdx = parent?.Children.IndexOf(current) ?? -1;
        if (currentIdx == -1) {
            return;
        }

        if (currentIdx + 1 < parent!.Children.Count) {
            Node nextNode = parent.Children[currentIdx + 1];
            if (nextNode.Tags["imapo.treenode"] is TreeGridNode next) {
                SelectedNode = next;
            }
        }
    }

    public void OpenImage()
    {
        if (SelectedNode is not { Kind: TreeGridNodeKind.Image }) {
            return;
        }

        if (!projectManager.IsValidImage(SelectedNode.Node.Path)) {
            string imagePath = SelectedNode.Node.Path["/root/".Length..];
            _ = MessageBox.Show(
                "Project does not have an associated PO file for this image. " +
                $"Please review the project file to ensure the following path is included: {imagePath}",
                "Invalid project configuration",
                MessageBoxType.Error);
            return;
        }

        openedNode = SelectedNode;

        DataStream stream = SelectedNode.Node.Stream!;
        stream.Position = 0;
        CurrentImage = new Bitmap(stream);

        PoEntry entry = projectManager.GetOrAddEntry(SelectedNode.Node.Path);
        ContextText = entry.Context;
        ImageText = entry.Original;
    }

    public void OnImageTextChange()
    {
        if (openedNode is null) {
            return;
        }

        projectManager.AddOrUpdateEntry(openedNode.Node.Path, ImageText);
    }

    public void OpenUploadScreenshot()
    {
        var upload = new ScreenshotUploadView(project!);
        upload.Show();
    }

    private void Quit() => Application.Instance.Quit();

    private void OpenAboutDialog()
    {
        var about = new AboutView();
        _ = about.ShowDialog(Application.Instance.MainForm);
    }
}
