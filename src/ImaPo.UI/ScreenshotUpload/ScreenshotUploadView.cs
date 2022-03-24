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
        var tokenTextBox = new TextBox();
        _ = tokenTextBox.TextBinding.BindDataContext((ScreenshotUploadViewModel vm) => vm.WeblateToken);

        var infoLayout = new TableLayout {
            Padding = new Padding(10),
            Spacing = new Size(5, 5),
            Rows = {
                new TableRow(new Label { Text = "API token:" }, tokenTextBox),
            },
        };

        var progressBar = new ProgressBar();
        var logsBox = new TextArea { ReadOnly = true };

        var uploadBtn = new Button {
            Text = "Upload!",
            Command = viewModel.UploadCommand,
        };

        return new StackLayout(infoLayout, progressBar, uploadBtn, logsBox) {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };
    }
}
