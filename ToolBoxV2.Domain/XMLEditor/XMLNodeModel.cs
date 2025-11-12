using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Domain.XMLEditor
{
    /// <summary>
    /// Represents a lightweight, tree-structured model of an XML document element.
    ///
    /// Layer role:
    /// Domain layer — pure data model with no dependencies on UI or external libraries.
    /// It acts as a neutral structure for transferring XML content between layers:
    /// - Infrastructure builds it from actual <see cref="System.Xml.Linq.XDocument"/> objects.
    /// - Presentation (UI) binds to it to display and navigate XML trees.
    /// - Application services may use it to locate or reference nodes by their generated IDs.
     
    public class XMLNodeModel
    {
        // A generated, stable path-like identifier (e.g. /Root[0]/Group[2]/Tag[0])
        // that uniquely identifies the node within the XML hierarchy.
        public string Id { get; set; } = string.Empty;         // generated id so UI can refer to it
        public string Name { get; set; } = string.Empty;        // element name (e.g. Screen, Group, Tag).

        // Collection of XML attributes for this element.
        // Key = attribute name, Value = attribute value
        // Example: Attributes["Caption"] = "Main screen"
        public Dictionary<string, string> Attributes { get; set; } = new();
        public List<XMLNodeModel> Children { get; set; } = new(); // Child elements of this node, preserving document order.

        // optional small text for UI
        public string? TextContent { get; set; }               // e.g. caption="..."

        // Indicates whether the node has any child elements.
        // Useful for quickly expanding/collapsing UI tree nodes.
        public bool HasChildren => Children.Count > 0;
    }
}
