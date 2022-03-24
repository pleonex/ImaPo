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
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using YamlDotNet.Serialization;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace ImaPo.UI.Projects;

public class ProjectManager
{
    private readonly Po2Binary po2Binary = new Po2Binary();

    public ProjectSettings Settings { get; private set; }

    public bool OpenedProject { get; private set; }

    public void OpenProject(string projectFile)
    {
        // Deserialize YAML file
        string projectText = File.ReadAllText(projectFile);
        Settings = new DeserializerBuilder()
            .Build()
            .Deserialize<ProjectSettings>(projectText);

        // Resolve relative paths to the folder of the project file
        string projectRoot = Path.GetDirectoryName(projectFile) ?? throw new FileNotFoundException("Invalid path");
        Settings.ImageFolder = Path.GetFullPath(Settings.ImageFolder, projectRoot);
        Settings.TextFolder = Path.GetFullPath(Settings.TextFolder, projectRoot);

        OpenedProject = true;
    }

    public Node CreateTranslationNodeHierarchy(TranslationUnit unit)
    {
        var translationNode = ReadOrCreateTranslationNode(unit);

        var imageDirInfo = new DirectoryInfoWrapper(new DirectoryInfo(Settings.ImageFolder));
        var foundFiles = unit.ImageGlobPattern.Execute(imageDirInfo)
            .Files
            .Select(f => f.Path);

        foreach (string foundFile in foundFiles) {
            string fullImagePath = Path.Combine(imageDirInfo.FullName, foundFile);
            string relativeImageDir = Path.GetDirectoryName(foundFile);

            var imageNode = NodeFactory.FromFile(fullImagePath, FileOpenMode.Read);
            NodeFactory.CreateContainersForChild(translationNode, relativeImageDir, imageNode);
        }

        return translationNode;
    }

    public bool HasComponentTextForImage(Node imageNode)
    {
        PoEntry? entry = GetTranslationEntryForImage(imageNode);
        return entry is not null && entry.Original != "TODO";
    }

    public PoEntry GetOrCreateEntry(Node imageNode)
    {
        Node poNode = FindComponentForImage(imageNode);
        Po po = poNode.GetFormatAs<Po>();

        PoEntry? entry = FindTranslationEntryForImage(poNode, imageNode);
        if (entry is null) {
            entry = new PoEntry("TODO") { Context = $"image={GetRelativePath(poNode, imageNode)}" };
            po.Add(entry);
            SaveComponent(poNode);
        }

        return entry;
    }

    public void AddOrUpdateEntry(Node imageNode, string text)
    {
        PoEntry entry = GetOrCreateEntry(imageNode);
        if (string.IsNullOrEmpty(text)) {
            // TODO: remove from PO
            entry.Original = "TODO";
        } else {
            entry.Original = text;
        }

        Node poNode = FindComponentForImage(imageNode);
        SaveComponent(poNode);
    }

    private Node ReadOrCreateTranslationNode(TranslationUnit unit)
    {
        string extension = Settings.GeneratePoTemplates ? ".pot" : ".po";
        string poPath = Path.Combine(Settings.TextFolder, unit.PoName + extension);

        if (!File.Exists(poPath)) {
            Po po = new Po(new PoHeader(Settings.Name, Settings.ContactAddress, Settings.Language));
            return new Node(unit.PoName + extension, po);
        }

        return NodeFactory.FromFile(poPath, FileOpenMode.Read)
            .TransformWith<Binary2Po>();
    }

    private PoEntry? GetTranslationEntryForImage(Node imageNode) =>
        FindTranslationEntryForImage(FindComponentForImage(imageNode), imageNode);

    private PoEntry? FindTranslationEntryForImage(Node translationNode, Node imageNode) =>
        translationNode.GetFormatAs<Po>()
            .Entries
            .FirstOrDefault(e => e.Context == $"image={GetRelativePath(translationNode, imageNode)}");

    private string GetRelativePath(Node translationNode, Node imageNode)
    {
        // Assuming PO contains image
        return imageNode.Path[(translationNode.Path.Length + NodeSystem.PathSeparator.Length) ..];
    }

    private Node FindComponentForImage(Node imageNode)
    {
        Node current = imageNode;
        while (current.Parent is not null) {
            if (current.Format is Po po) {
                return current;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException($"Cannot find PO for image: {imageNode.Path}");
    }

    private void SaveComponent(Node translationNode)
    {
        string poPath = Path.Combine(Settings.TextFolder, translationNode.Name);
        using BinaryFormat binary = po2Binary.Convert(translationNode.GetFormatAs<Po>());
        using var fileStream = new FileStream(poPath, FileMode.Create);
        binary.Stream.WriteTo(fileStream);
    }
}
