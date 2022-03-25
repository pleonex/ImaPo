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
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using YamlDotNet.Serialization;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace ImaPo.UI.Projects;

public sealed class ProjectManager : IDisposable
{
    private const string TodoText = "TODO";
    private readonly Po2Binary po2Binary = new Po2Binary();

    public ProjectSettings? Settings { get; private set; }

    public bool OpenedProject { get; private set; }

    public Node? Root { get; private set; }

    public bool Disposed { get; private set; }

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

        // Create project tree
        Root?.Dispose();
        Root = new Node("root");
        foreach (TranslationUnit unit in Settings.Units) {
            Root.Add(CreateUnitTree(unit));
        }

        OpenedProject = true;
    }

    public bool HasSegmentForImage(Node imageNode)
    {
        PoEntry? entry = FindSegmentForImage(imageNode);
        return entry is not null && entry.Original != TodoText;
    }

    public PoEntry GetOrAddSegment(Node imageNode)
    {
        Node poNode = FindUnitForImage(imageNode);
        Po po = poNode.GetFormatAs<Po>();

        PoEntry? entry = FindSegmentForImage(poNode, imageNode);
        if (entry is null) {
            entry = new PoEntry(TodoText) { Context = $"image={GetImageRelativePath(poNode, imageNode)}" };
            po.Add(entry);
            SaveUnit(poNode);
        }

        return entry;
    }

    public void AddOrUpdateSegment(Node imageNode, string text)
    {
        PoEntry entry = GetOrAddSegment(imageNode);
        if (string.IsNullOrEmpty(text)) {
            // TODO: remove from PO
            entry.Original = TodoText;
        } else {
            entry.Original = text;
        }

        Node poNode = FindUnitForImage(imageNode);
        SaveUnit(poNode);
    }

    public void Dispose()
    {
        if (Disposed) {
            return;
        }

        Root?.Dispose();
        GC.SuppressFinalize(this);
        Disposed = true;
    }

    private Node CreateUnitTree(TranslationUnit unit)
    {
        var unitNode = ReadOrCreateUnitNode(unit);

        var imageDirInfo = new DirectoryInfoWrapper(new DirectoryInfo(Settings.ImageFolder));
        var matcher = new Matcher().AddInclude(unit.ImagesGlobPattern);
        var matchedFiles = matcher.Execute(imageDirInfo).Files.Select(f => f.Path);

        foreach (string filePath in matchedFiles) {
            string fullImagePath = Path.Combine(imageDirInfo.FullName, filePath);
            string relativeImageDir = Path.GetDirectoryName(filePath);

            var imageNode = NodeFactory.FromFile(fullImagePath, FileOpenMode.Read);
            NodeFactory.CreateContainersForChild(unitNode, relativeImageDir, imageNode);
        }

        return unitNode;
    }

    private Node ReadOrCreateUnitNode(TranslationUnit unit)
    {
        string extension = Settings.GeneratePoTemplates ? ".pot" : ".po";
        string poPath = Path.Combine(Settings.TextFolder, unit.Name + extension);

        if (!File.Exists(poPath)) {
            Po po = new Po(new PoHeader(Settings.Name, Settings.ContactAddress, Settings.Language));
            return new Node(unit.Name + extension, po);
        }

        return NodeFactory.FromFile(poPath, FileOpenMode.Read)
            .TransformWith<Binary2Po>();
    }

    private PoEntry? FindSegmentForImage(Node imageNode) =>
        FindSegmentForImage(FindUnitForImage(imageNode), imageNode);

    private PoEntry? FindSegmentForImage(Node unitNode, Node imageNode) =>
        unitNode.GetFormatAs<Po>()
            .Entries
            .FirstOrDefault(e => e.Context == $"image={GetImageRelativePath(unitNode, imageNode)}");

    private string GetImageRelativePath(Node unitNode, Node imageNode)
    {
        // Assuming PO contains image
        return imageNode.Path[(unitNode.Path.Length + NodeSystem.PathSeparator.Length) ..];
    }

    private Node FindUnitForImage(Node imageNode)
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

    private void SaveUnit(Node unitNode)
    {
        string poPath = Path.Combine(Settings.TextFolder, unitNode.Name);
        using BinaryFormat binary = po2Binary.Convert(unitNode.GetFormatAs<Po>());
        using var fileStream = new FileStream(poPath, FileMode.Create);
        binary.Stream.WriteTo(fileStream);
    }
}
