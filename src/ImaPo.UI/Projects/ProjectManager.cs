using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace ImaPo.UI.Projects;

public class ProjectManager
{
    private readonly ProjectSettings settings;
    private readonly Binary2Po binary2Po;
    private readonly Po2Binary po2Binary;
    private readonly Dictionary<string, Po> openedPos;

    public ProjectManager(ProjectSettings settings)
    {
        this.settings = settings;
        binary2Po = new Binary2Po();
        po2Binary = new Po2Binary();
        openedPos = new Dictionary<string, Po>();
    }

    public string GetTextExtension() => settings.GeneratePoTemplates ? ".pot" : ".po";

    public string GetRelativePath(string path) => path["/root/Images/".Length ..];

    public string GetRelatedPoPath(string path)
    {
        string? poName = null;
        string relativePath = GetRelativePath(path);
        foreach ((string key, string value) in settings.TextToImageFolderMapping) {
            if (relativePath.StartsWith(key, StringComparison.Ordinal)) {
                poName = value;
            }
        }

        if (poName is null) {
            throw new FileNotFoundException($"Missing entry to map: {path}");
        }

        return Path.Combine(settings.TextFolder, poName + GetTextExtension());
    }

    public Po GetRelatedPo(string path)
    {
        string filePath = GetRelatedPoPath(path);
        if (openedPos.TryGetValue(filePath, out Po? existingPo)) {
            return existingPo;
        }

        Po po;
        if (!File.Exists(filePath)) {
            po = new Po(new PoHeader(settings.Name, "author", "en"));
        } else {
            using var binary = new BinaryFormat(DataStreamFactory.FromFile(filePath, FileOpenMode.Read));
            po = binary2Po.Convert(binary);
        }

        openedPos[filePath] = po;
        return po;
    }

    public PoEntry? GetEntry(string path) =>
        GetEntry(GetRelatedPo(path), path);

    public PoEntry? GetEntry(Po po, string path) =>
        po.Entries.FirstOrDefault(e => e.Context == $"image={GetRelativePath(path)}");

    public PoEntry GetOrAddEntry(string path)
    {
        Po po = GetRelatedPo(path);
        PoEntry? entry = GetEntry(po, path);
        if (entry is null) {
            entry = new PoEntry("TODO") { Context = $"image={GetRelativePath(path)}" };
            po.Add(entry);
            SavePo(path);
        }

        return entry;
    }

    public bool HasEntry(string path)
    {
        PoEntry? entry = GetEntry(path);
        return entry is not null && entry.Original != "TODO";
    }

    public void AddOrUpdateEntry(string path, string text)
    {
        PoEntry entry = GetOrAddEntry(path);

        if (string.IsNullOrEmpty(text)) {
            // TODO: remove from PO
            entry.Original = "TODO";
        } else {
            entry.Original = text;
        }

        SavePo(path);
    }

    public void SavePo(string path)
    {
        string poPath = GetRelatedPoPath(path);
        Po po = GetRelatedPo(path);
        using BinaryFormat binary = po2Binary.Convert(po);
        using var fileStream = new FileStream(poPath, FileMode.Create);
        binary.Stream.WriteTo(fileStream);
    }
}
