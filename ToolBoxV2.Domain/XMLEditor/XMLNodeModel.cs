using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Domain.XMLEditor
{
    public class XMLNodeModel
    {
        public string Id { get; set; } = string.Empty;         // generated id so UI can refer to it
        public string Name { get; set; } = string.Empty;        // element name
        public Dictionary<string, string> Attributes { get; set; } = new();
        public List<XMLNodeModel> Children { get; set; } = new();

        // optional small text for UI
        public string? TextContent { get; set; }               // e.g. caption="..."

        // this is NOT for editing, just display/preview
        public bool HasChildren => Children.Count > 0;
    }
}
