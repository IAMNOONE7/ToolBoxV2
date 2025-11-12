using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Application.XMLEditor
{
    /// <summary>
    /// Concrete implementation of <see cref="IXMLTemplateService"/> that performs
    /// simple text replacement for placeholders inside XML templates.
    ///
    /// Layer role:
    /// Infrastructure layer — provides the actual algorithm for
    /// placeholder resolution (string manipulation), while the Application layer
    /// only defines the contract (<see cref="IXMLTemplateService"/>).
    ///
    /// Usage:
    /// Used by <see cref="XMLNodeEditService"/> when generating or updating nodes.
    /// Each placeholder in the XML fragment (e.g. [TagName]) is replaced with
    /// the corresponding value from the current Excel row.
    /// </summary>
    public class XMLTemplateService : IXMLTemplateService
    {
        // Replaces placeholders in the given XML template with actual values.
        // The template XML text containing placeholders in the form [ColumnName].
        // Dictionary mapping placeholder names to replacement strings.
        // Missing keys will leave placeholders as empty.
        public string ApplyPlaceholders(string templateXml, IDictionary<string, string> values)
        {
            var output = templateXml;
            foreach (var kvp in values)
            {
                var placeholder = $"[{kvp.Key}]";
                output = output.Replace(placeholder, kvp.Value ?? string.Empty);
            }
            return output;
        }
    }
}
