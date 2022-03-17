using System;
using System.Linq;
using Eto.Forms;
using Yarhl.FileSystem;

namespace ImaPo.UI.Main;

public class TreeGridNode : TreeGridItem
{
    public TreeGridNode(Node node)
        : base()
    {
        Node = node;

        var children = node.Children.OrderBy(c => !c.IsContainer).ThenBy(c => c.Name);
        foreach (var childNode in children) {
            var child = new TreeGridNode(childNode);
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
                return "\uf779";
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
        var child = new TreeGridNode(node);
        Children.Add(child);
        Node.Add(node);
    }

    public void UpdateChildren()
    {
        Children.Clear();

        var children = Node.Children.OrderBy(c => !c.IsContainer);
        foreach (var childNode in children) {
            var child = new TreeGridNode(childNode);
            Children.Add(child);
        }
    }
}
