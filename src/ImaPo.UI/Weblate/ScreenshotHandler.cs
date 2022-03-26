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
using System.Net.Http;
using System.Threading.Tasks;
using ImaPo.UI.Weblate.Models;

namespace ImaPo.UI.Weblate;

public class ScreenshotHandler : WeblateBaseHandler
{
    internal ScreenshotHandler(HttpClient client)
        : base(client)
    {
    }

    public IAsyncEnumerable<ScreenshotInfo> GetScreenshotsAsync() =>
        FetchListAsync<ScreenshotInfo>("api/screenshots/");

    public async Task<ScreenshotInfo> UploadScreenshotAsync(string image, string name, string project, string component, string language)
    {
        using var formData = new MultipartFormDataContent();

        // form: image -> data stream + file name with extension
        await using var fileStream = new FileStream(image, FileMode.Open);
        using var streamContent = new StreamContent(fileStream);
        formData.Add(streamContent, "image", Path.GetFileName(image));

        // form: name -> file name
        using var nameContent = new StringContent(name);
        formData.Add(nameContent, "name");

        // form: project_slug -> project slug
        using var projectSlugContent = new StringContent(project);
        formData.Add(projectSlugContent, "project_slug");

        // form: component_slug -> component slug
        using var componentSlugContent = new StringContent(component);
        formData.Add(componentSlugContent, "component_slug");

        // form: language_code -> language code
        using var languageContent = new StringContent(language);
        formData.Add(languageContent, "language_code");

        HttpResponseMessage response = await Client.PostAsync("api/screenshots/", formData).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return DeserializeJson<ScreenshotInfo>(responseText);
    }

    public async Task AssignUnitToScreenshotAsync(string screenshotId, int unitId)
    {
        using var formData = new MultipartFormDataContent();

        using var unitContent = new StringContent(unitId.ToString());
        formData.Add(unitContent, "unit_id");

        string request = $"api/screenshots/{screenshotId}/units/";
        HttpResponseMessage response = await Client.PostAsync(request, formData).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
    }
}
