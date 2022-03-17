using System;
using System.IO;
using System.Windows.Input;
using Eto.Drawing;
using Eto.Forms;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Yarhl.FileSystem;
using Yarhl.IO;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace ImaPo.UI.Main;

public sealed class MainViewModel : ObservableObject
{
    private TreeGridNode selectedNode;
    private Image currentImage;
    private string contextText;
    private string imageText;

    public MainViewModel()
    {
        QuitCommand = new RelayCommand(Quit);
        AboutCommand = new RelayCommand(OpenAboutDialog);
        OpenFolderCommand = new RelayCommand(OpenFolder);
        OpenImageCommand = new RelayCommand(OpenImage);

        RootNode = new TreeGridNode(NodeFactory.CreateContainer("root"));
    }

    public event EventHandler<TreeGridNode> OnNodeUpdate;

    public ICommand QuitCommand { get; }

    public ICommand AboutCommand { get; }

    public ICommand OpenFolderCommand { get; }

    public ICommand OpenImageCommand { get; }

    public TreeGridNode SelectedNode {
        get => selectedNode;
        set => SetProperty(ref selectedNode, value);
    }

    public TreeGridNode RootNode { get; }

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
        set => SetProperty(ref imageText, value);
    }

    public void OpenFolder()
    {
        RootNode.Node.RemoveChildren();

        var dialog = new SelectFolderDialog {
            Title = "Add external folders",
        };
        if (dialog.ShowDialog(Application.Instance.MainForm) != DialogResult.Ok) {
            return;
        }

        string name = Path.GetFileName(dialog.Directory!);
        Node node = NodeFactory.FromDirectory(dialog.Directory!, "*", name, subDirectories: true);
        RootNode.Add(node);
        OnNodeUpdate?.Invoke(this, RootNode);
    }

    public void OpenImage()
    {
        if (!SelectedNode.Node.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        DataStream stream = SelectedNode.Node.Stream!;
        stream.Position = 0;
        CurrentImage = new Bitmap(stream);

        ContextText = $"image={SelectedNode.Node.Path}";
    }

    private void Quit() => Application.Instance.Quit();

    private void OpenAboutDialog()
    {
        var about = new AboutView();
        _ = about.ShowDialog(Application.Instance.MainForm);
    }
}
