using System.Collections.Generic;

namespace ImaPo.UI.Projects;

public class ProjectSettings
{
    public string Name { get; set; }

    public string ContactAddress { get; set; }

    public string Language { get; set; }

    public string WeblateUrl { get; set; }

    public string WeblateProjectSlug { get; set; }

    public string TextFolder { get; set; }

    public string ImageFolder { get; set; }

    public bool GeneratePoTemplates { get; set; }

    public Dictionary<string, string> TextToImageFolderMapping { get; set; }
}
