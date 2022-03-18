using System;
using System.IO;
using System.Windows.Input;
using Eto.Drawing;
using Eto.Forms;
using ImaPo.UI.Projects;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using YamlDotNet.Serialization;
using Yarhl.FileSystem;
using Yarhl.IO;
using RelayCommand = Microsoft.Toolkit.Mvvm.Input.RelayCommand;

namespace ImaPo.UI.Main;

public sealed class MainViewModel : ObservableObject
{
    private string projectPath;
    private ProjectSettings project;
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

        RootNode = new TreeGridNode(NodeFactory.CreateContainer("root"));
    }

    public event EventHandler<TreeGridNode> OnNodeUpdate;

    public ICommand QuitCommand { get; }

    public ICommand AboutCommand { get; }

    public ICommand NewProjectCommand { get; }

    public ICommand OpenProjectCommand { get; }

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

        string projectText = File.ReadAllText(dialog.FileName);
        project = new DeserializerBuilder()
            .Build()
            .Deserialize<ProjectSettings>(projectText);

        projectPath = Path.GetDirectoryName(dialog.FileName) ?? throw new FileNotFoundException("Invalid path");
        string imagePath = Path.Combine(projectPath, project.ImageFolder);
        string textPath = Path.Combine(projectPath, project.TextFolder);
        if (!Directory.Exists(textPath)) {
            _ = Directory.CreateDirectory(textPath);
        }

        RootNode.Node.RemoveChildren();
        RootNode.Add(NodeFactory.FromDirectory(imagePath, "*.png", "Images", subDirectories: true));
        RootNode.Add(NodeFactory.FromDirectory(textPath, "*.po*", "Texts", subDirectories: true));

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
