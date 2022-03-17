using System;
using System.Diagnostics.CodeAnalysis;
using Eto.Drawing;
using Eto.Forms;

namespace ImaPo.UI.Main;

public sealed class AboutView : AboutDialog
{
    [SuppressMessage("", "S1075", Justification = "Project URL is ok to hard-code")]
    public AboutView()
    {
        // Logo = Bitmap.FromResource(ResourcesName.Icon)
        WebsiteLabel = "ImaPo website";
        Website = new Uri("https://pleonex.dev/ImaPo/");
        Developers = new[] { "Benito Palacios Sánchez" };
        License = "MIT License\nhttps://opensource.org/licenses/MIT";
        ProgramName = "ImaPo";
        ProgramDescription = "Tool to help to translate images via Po files in Weblate.";
    }
}
