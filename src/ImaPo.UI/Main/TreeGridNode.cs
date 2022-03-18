using System;
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
        node.Tags["imapo.treenode"] = this;

        var children = node.Children
            .OrderBy(c => !c.IsContainer)
            .ThenBy(c => c.Name);
        foreach (var childNode in children) {
            var child = new TreeGridNode(childNode, projectManager);
            Children.Add(child);
        }
    }

    public Node Node { get; }

    public string QualifiedName => $"{Icon} {Node.Name}";

    public string Icon {
        get {
            if (Node.Format is null || Node.Format is NodeContainerFormat) {
                return "\uf74a";
            }

            if (Node.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) {
                string status = projectManager.HasEntry(Node.Path) ? "\uf00c " : "\uf00d ";
                return status + "\uf779";
            }

            if (Node.Name.EndsWith(".po", StringComparison.OrdinalIgnoreCase)) {
                return "\ufac9";
            }

            if (Node.Name.EndsWith(".pot", StringComparison.OrdinalIgnoreCase)) {
                return "\ufac9";
            }

            return "\uf471";
        }
    }

    public void Add(Node node)
    {
        var child = new TreeGridNode(node, projectManager);
        Children.Add(child);
        Node.Add(node);
    }

    public void UpdateChildren()
    {
        Children.Clear();

        var children = Node.Children.OrderBy(c => !c.IsContainer);
        foreach (var childNode in children) {
            var child = new TreeGridNode(childNode, projectManager);
            Children.Add(child);
        }
    }
}
