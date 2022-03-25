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
using System.IO;
using System.Linq;
using Eto.Forms;
using ImaPo.UI.Projects;
using Yarhl.FileSystem;

namespace ImaPo.UI.Main;

public class TreeGridNode : TreeGridItem
{
    private readonly ProjectManager projectManager;

    public TreeGridNode(Node node, ProjectManager projectManager)
    {
        this.projectManager = projectManager;
        Node = node;
        Kind = GetKind(node);

        IOrderedEnumerable<Node> children = node.Children
            .OrderBy(c => !c.IsContainer)
            .ThenBy(c => c.Name);
        foreach (Node childNode in children) {
            var childTree = new TreeGridNode(childNode, projectManager);
            Children.Add(childTree);
        }
    }

    public Node Node { get; }

    public TreeGridNodeKind Kind { get; }

    public string QualifiedName => $"{Icon} {Node.Name}";

    private string Icon =>
        Kind switch {
            TreeGridNodeKind.Folder => "\uf74a", // folder icon
            TreeGridNodeKind.Image => $"{TranslationStatusIcon} \uf03e", // image icon
            TreeGridNodeKind.Translation => "\ufac9", // translate icon
            TreeGridNodeKind.Unknown => "\uf471", // binary icon
            _ => "\uf128", // '?'
        };

    private string TranslationStatusIcon =>
        projectManager.HasSegmentForImage(Node) ? "\uf00c" : "\uf00d"; // tick or cross

    private static TreeGridNodeKind GetKind(Node node)
    {
        if (node.Format is null or NodeContainerFormat) {
            return TreeGridNodeKind.Folder;
        }

        string extension = Path.GetExtension(node.Name).ToUpperInvariant();
        return extension switch {
            ".PNG" => TreeGridNodeKind.Image,
            ".JPG" or ".JPEG" => TreeGridNodeKind.Image,
            ".BMP" => TreeGridNodeKind.Image,
            ".TIFF" => TreeGridNodeKind.Image,
            ".GIF" => TreeGridNodeKind.Image,

            ".PO" or ".POT" => TreeGridNodeKind.Translation,
            _ => TreeGridNodeKind.Unknown,
        };
    }
}
