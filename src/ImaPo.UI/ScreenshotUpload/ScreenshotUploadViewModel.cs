using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using ImaPo.UI.Projects;
using ImaPo.UI.ScreenshotUpload.Weblate;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace ImaPo.UI.ScreenshotUpload;

public class ScreenshotUploadViewModel : ObservableObject
{
    private readonly HttpClient client;
    private readonly ProjectSettings project;

    public ScreenshotUploadViewModel(ProjectSettings project)
    {
        this.project = project;

        client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("User-Agent", "ImaPo");

        UploadCommand = new AsyncRelayCommand(UploadAsync);
    }

    public ICommand UploadCommand { get; }

    public string WeblateToken { get; set; }

    public async Task UploadAsync()
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", WeblateToken);

        try {
            foreach (string component in GetComponents()) {
                await foreach (Unit unit in GetUnitsAsync(component).ConfigureAwait(false)) {
                    string imagePath = unit.Context["image=".Length..];

                    string screenshotId = await UploadScreenshot(imagePath, component).ConfigureAwait(false);

                    await AssignUnitToScreenshot(screenshotId, unit.Id).ConfigureAwait(false);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex);
        }
    }

    private IEnumerable<string> GetComponents()
    {
        foreach ((string name, _) in project.TextToImageFolderMapping) {
            // TODO: find component name
            yield return "images" + name;
        }
    }

    private async IAsyncEnumerable<Unit> GetUnitsAsync(string component)
    {
        Console.WriteLine($"Querying component: {component}");
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var baseUri = new Uri(project.WeblateUrl);
        var requestUri = new Uri(baseUri, $"api/translations/{project.WeblateProjectSlug}/{component}/{project.Language}/units/");

        bool moreRequests;
        do {
            Console.WriteLine($"Request to: {requestUri}");
            Stream result = await client.GetStreamAsync(requestUri).ConfigureAwait(false);
            UnitQueryResponse response = await JsonSerializer.DeserializeAsync<UnitQueryResponse>(result, jsonOptions)
                .ConfigureAwait(false);
            if (response is null) {
                yield break;
            }

            foreach (Unit unit in response.Results) {
                yield return unit;
            }

            moreRequests = !string.IsNullOrEmpty(response.Next);
            if (moreRequests) {
                requestUri = new Uri(response.Next);
            }
        } while (moreRequests);
    }

    private async Task<string> UploadScreenshot(string relativePath, string component)
    {
        string file = Path.Combine(project.ImageFolder, relativePath);
        Console.WriteLine($"Uploading screenshot: {file}");
        using var formData = new MultipartFormDataContent();

        // form: image -> data stream + file name with extension
        await using var fileStream = new FileStream(file, FileMode.Open);
        using var streamContent = new StreamContent(fileStream);
        formData.Add(streamContent, "image", Path.GetFileName(file));

        // form: name -> file name
        using var nameContent = new StringContent(relativePath.Replace("/", "_"));
        formData.Add(nameContent, "name");

        // form: project_slug -> project slug
        using var projectSlugContent = new StringContent(project.WeblateProjectSlug);
        formData.Add(projectSlugContent, "project_slug");

        // form: component_slug -> component slug
        using var componentSlugContent = new StringContent(component);
        formData.Add(componentSlugContent, "component_slug");

        // form: language_code -> language code
        using var languageContent = new StringContent(project.Language);
        formData.Add(languageContent, "language_code");

        var baseUri = new Uri(project.WeblateUrl);
        var requestUri = new Uri(baseUri, $"api/screenshots/");
        HttpResponseMessage response = await client.PostAsync(requestUri, formData).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();

        string responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        ScreenshotCreateResult createResult = JsonSerializer.Deserialize<ScreenshotCreateResult>(responseText, jsonOptions);

        // URL is the same as request appended the new ID
        string url = createResult?.Url ?? throw new FormatException();
        return url.Replace(requestUri.AbsoluteUri, string.Empty)[..^1];
    }

    private async Task AssignUnitToScreenshot(string screenshotId, int unitId)
    {
        Console.WriteLine($"Assigning ID: {unitId} to {screenshotId}");
        using var formData = new MultipartFormDataContent();

        using var unitContent = new StringContent(unitId.ToString());
        formData.Add(unitContent, "unit_id");

        var baseUri = new Uri(project.WeblateUrl);
        var requestUri = new Uri(baseUri, $"/api/screenshots/{screenshotId}/units/");
        HttpResponseMessage response = await client.PostAsync(requestUri, formData).ConfigureAwait(false);
        _ = response.EnsureSuccessStatusCode();
    }
}
