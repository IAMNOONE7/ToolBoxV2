using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Domain.XMLEditor
{
    public class XMLKeyDefinition
    {
        // e.g. "group" – the element type we’re repeating/updating
        public string ElementName { get; set; }

        // e.g. "name" – the attribute that will have [key] in the template
        public string AttributeName { get; set; }
    }
}
