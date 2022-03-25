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
using Eto.Drawing;
using Eto.Forms;
using ImaPo.UI.Projects;

namespace ImaPo.UI.ScreenshotUpload;

public class ScreenshotUploadView : Dialog
{
    private readonly ScreenshotUploadViewModel viewModel;

    public ScreenshotUploadView(ProjectSettings project)
    {
        viewModel = new ScreenshotUploadViewModel(project);
        DataContext = viewModel;

        Title = "Upload images to Weblate";
        Size = new Size(500, 250);
        Content = InitializeComponents();
    }

    private Control InitializeComponents()
    {
        var tokenTextBox = new TextBox { PlaceholderText = "Token" };
        _ = tokenTextBox.TextBinding.BindDataContext((ScreenshotUploadViewModel vm) => vm.WeblateToken);

        var infoLayout = new TableLayout {
            Spacing = new Size(5, 5),
            Rows = {
                new TableRow(
                    new Label { Text = "API token:", VerticalAlignment = VerticalAlignment.Center },
                    tokenTextBox),
            },
        };

        var outputText = new TextArea {
            ReadOnly = true,
        };
        viewModel.StatusUpdated += (_, e) => outputText.Text += $"{e}\n";

        var progressBar = new ProgressBar {
            Indeterminate = true,
        };
        progressBar.BindDataContext(p => p.Visible, (ScreenshotUploadViewModel vm) => vm.UploadCommand.IsRunning);

        var uploadBtn = new Button {
            Text = "Upload!",
            Command = viewModel.UploadCommand,
        };

        return new StackLayout(infoLayout, new StackLayoutItem(outputText, true), progressBar, uploadBtn) {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = 5,
            Spacing = 5,
        };
    }
}
