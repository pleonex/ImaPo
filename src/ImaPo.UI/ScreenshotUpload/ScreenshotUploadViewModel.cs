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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ImaPo.UI.Projects;
using ImaPo.UI.Weblate;
using ImaPo.UI.Weblate.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace ImaPo.UI.ScreenshotUpload;

public class ScreenshotUploadViewModel : ObservableObject
{
    private readonly ProjectSettings project;
    private readonly WeblateClient client;
    private string token;

    public ScreenshotUploadViewModel(ProjectSettings project)
    {
        this.project = project;
        client = new WeblateClient(new Uri(project.WeblateUrl));

        UploadCommand = new AsyncRelayCommand(UploadAsync, () => !string.IsNullOrWhiteSpace(WeblateToken));
    }

    public event EventHandler<string> StatusUpdated;

    public AsyncRelayCommand UploadCommand { get; }

    public string WeblateToken {
        get => token;
        set {
            if (SetProperty(ref token, value)) {
                UploadCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public async Task UploadAsync()
    {
        client.SetToken(WeblateToken);

        try {
            var components = project.Units.Select(e => e.WeblateComponentSlug);
            foreach (string component in components) {
                OnNewStatus($"Querying existing screenshots for '{component}'");
                var availableScreenshots = await client.Components.GetScreenshotsAsync(project.WeblateProjectSlug, component)
                    .ToListAsync().ConfigureAwait(false);
                OnNewStatus($"Found '{availableScreenshots.Count} screenshots in this component");

                OnNewStatus("Querying units for the component");
                var units = client.Units.GetUnitsAsync(project.WeblateProjectSlug, component, project.Language);
                await foreach (Unit unit in units.ConfigureAwait(false)) {
                    string imagePath = unit.Context["image=".Length..];
                    string filename = imagePath.Replace("/", "_");
                    string file = Path.Combine(project.ImageFolder, imagePath);

                    ScreenshotInfo? screenshotInfo = availableScreenshots.FirstOrDefault(s => s.Name == filename);
                    if (screenshotInfo is null) {
                        OnNewStatus($"Uploading screenshot: '{filename}' from '{file}' for '{unit.Id}'");
                        screenshotInfo = await client.Screenshots.UploadScreenshotAsync(
                            file,
                            filename,
                            project.WeblateProjectSlug,
                            component,
                            project.Language).ConfigureAwait(false);
                    } else {
                        OnNewStatus($"Screenshot already exists for unit '{unit.Id}'");
                    }

                    if (screenshotInfo.Units.Any(u => u == unit.SourceUnit)) {
                        OnNewStatus("Screenshot already linked to unit");
                    } else {
                        OnNewStatus($"Assigning unit: '{unit.Id}' to screenshot '{screenshotInfo.ScreenshotId}'");
                        await client.Screenshots.AssignUnitToScreenshotAsync(screenshotInfo.ScreenshotId, unit.Id).ConfigureAwait(false);
                    }
                }
            }
        } catch (Exception ex) {
            OnNewStatus(ex.ToString());
        }
    }

    private void OnNewStatus(string status)
    {
        Eto.Forms.Application.Instance.Invoke(() => StatusUpdated?.Invoke(this, status));
    }
}
