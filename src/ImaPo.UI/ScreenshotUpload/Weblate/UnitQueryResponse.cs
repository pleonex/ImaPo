using System.Collections.ObjectModel;

namespace ImaPo.UI.ScreenshotUpload.Weblate;

public class UnitQueryResponse
{
    public int Count { get; set; }

    public string Next { get; set; }

    public string Previous { get; set; }

    public Collection<Unit> Results { get; set; } = new Collection<Unit>();
}
