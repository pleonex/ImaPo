using System;
using System.Linq.Expressions;
using Eto.Drawing;
using Eto.Forms;
using ImaPo.UI.Projects;

namespace ImaPo.UI.ScreenshotUpload;

public class ScreenshotUploadView : Form
{
    private readonly ScreenshotUploadViewModel viewModel;

    public ScreenshotUploadView(ProjectSettings project)
    {
        viewModel = new ScreenshotUploadViewModel(project);
        DataContext = viewModel;

        Size = new Size(600, 250);
        Content = InitializeComponents();
    }

    private Control InitializeComponents()
    {
        var infoLayout = new TableLayout {
            Padding = new Padding(10),
            Spacing = new Size(5, 5),
            Rows = {
                CreateLabelInputRow("API token", string.Empty, vm => vm.WeblateToken),
            },
        };

        var progressBar = new ProgressBar();
        var logsBox = new TextArea { ReadOnly = true };

        var uploadBtn = new Button {
            Text = "Upload!",
            Command = viewModel.UploadCommand,
        };

        var layout = new StackLayout(infoLayout, progressBar, uploadBtn, logsBox) {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };
        return layout;
    }

    private TableRow CreateLabelInputRow(string label, string placeholder, Expression<Func<ScreenshotUploadViewModel, string>> binding)
    {
        var textBox = new TextBox { PlaceholderText = placeholder };
        _ = textBox.TextBinding.BindDataContext(binding);

        return new TableRow(
            new Label { Text = label },
            textBox);
    }
}
