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
using System.Windows.Input;
using Eto.Drawing;
using Eto.Forms;
using ImaPo.UI.Projects;
using ImaPo.UI.ScreenshotUpload;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace ImaPo.UI.Main;

public sealed class MainViewModel : ObservableObject
{
    private readonly ProjectManager projectManager;

    private Node? openedNode;
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
        UploadScreenshotCommand = new RelayCommand(OpenUploadScreenshot, () => projectManager.OpenedProject);

        projectManager = new ProjectManager();
    }

    public event EventHandler<TreeGridNode?> OnNodeUpdate;

    public ICommand QuitCommand { get; }

    public ICommand AboutCommand { get; }

    public ICommand NewProjectCommand { get; }

    public ICommand OpenProjectCommand { get; }

    public ICommand OpenImageCommand { get; }

    public RelayCommand UploadScreenshotCommand { get; }

    public TreeGridNode? SelectedNode {
        get => selectedNode;
        set => SetProperty(ref selectedNode, value);
    }

    public TreeGridNode? RootNode { get; set; }

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
            CheckFileExists = true,
        };
        if (dialog.ShowDialog(Application.Instance.MainForm) != DialogResult.Ok) {
            return;
        }

        try {
            projectManager.OpenProject(dialog.FileName);
        } catch (Exception ex) {
            _ = MessageBox.Show($"Error opening project file: {ex}", MessageBoxType.Error);
            return;
        }

        RootNode = new TreeGridNode(projectManager.Root, projectManager);
        OnNodeUpdate?.Invoke(this, RootNode);

        UploadScreenshotCommand.NotifyCanExecuteChanged();
    }

    public void OpenImage()
    {
        if (SelectedNode is not { Kind: TreeGridNodeKind.Image }) {
            return;
        }

        if (SelectedNode.Node.Stream is null) {
            _ = MessageBox.Show($"Error opening image. Unknown node format??", MessageBoxType.Error);
            return;
        }

        openedNode = SelectedNode.Node;

        DataStream stream = openedNode.Stream!;
        stream.Position = 0;

        Image? oldImage = CurrentImage;
        try {
            CurrentImage = new Bitmap(stream);
        } catch {
            CurrentImage = null;
        }

        oldImage?.Dispose();

        PoEntry entry = projectManager.GetOrAddSegment(openedNode);
        ContextText = entry.Context;
        ImageText = entry.Original;
    }

    public void OnImageTextChange()
    {
        if (openedNode is null) {
            return;
        }

        projectManager.AddOrUpdateSegment(openedNode, ImageText);
    }

    private void OpenUploadScreenshot()
    {
        using var upload = new ScreenshotUploadView(projectManager.Settings);
        upload.ShowModal();
    }

    private void Quit() => Application.Instance.Quit();

    private void OpenAboutDialog()
    {
        using var about = new AboutView();
        _ = about.ShowDialog(Application.Instance.MainForm);
    }
}
