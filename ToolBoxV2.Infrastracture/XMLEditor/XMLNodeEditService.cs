using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Domain.XMLEditor;
using static ToolBoxV2.Application.XMLEditor.IXMLNodeEditService;

namespace ToolBoxV2.Application.XMLEditor
{
    /// <summary>
    /// Concrete implementation of <see cref="IXMLNodeEditService"/> that applies
    /// tabular (Excel) data to an XDocument either by generating new nodes
    /// or updating existing ones identified by a key.
    ///
    /// Layer &amp; role:
    /// Infrastructure — contains the “how” of XML manipulation.
    /// The Application layer exposes the contract (<see cref="IXMLNodeEditService"/>),
    /// while this class performs the actual edits on the XML tree.
    /// </summary>
    
    public class XMLNodeEditService : IXMLNodeEditService
    {
        private readonly IXMLTemplateService _templateService;

        /// Injects an <see cref="IXMLTemplateService"/> used to fill template placeholders.
        public XMLNodeEditService(IXMLTemplateService templateService)
        {
            _templateService = templateService;
        }

        // Applies Excel-like rows to the selected XML node using the specified mode.
        public XDocument Apply(
            XDocument sourceDocument,
            string selectedNodeId,
            string templateXml,
            IEnumerable<IDictionary<string, string>> excelRows,
            XMLEditMode mode,
            XMLKeyDefinition keyDef = null)
        {
            var root = sourceDocument.Root ?? throw new InvalidOperationException("XML has no root.");

            // 1) find the selected element in the original doc
            var selectedElement = FindByGeneratedId(root, selectedNodeId, "");
            if (selectedElement == null)
                throw new InvalidOperationException("Selected element not found.");

            switch (mode)
            {
                case XMLEditMode.Generate:
                    return ApplyGenerateMode(sourceDocument, selectedElement, templateXml, excelRows);

                case XMLEditMode.Update:
                    if (keyDef == null)
                        throw new ArgumentNullException(nameof(keyDef), "Key definition is required for update mode.");
                    return ApplyUpdateMode(sourceDocument, selectedElement, templateXml, excelRows, keyDef);

                default:
                    throw new NotSupportedException($"Mode {mode} is not supported.");
            }
        }

        // ==========================================================
        // GENERATE MODE
        // ==========================================================        
        // For each row, fills templateXml and appends a new element
        // in place of the original selected element (the selected element is replaced
        // by a sequence of newly generated siblings).
        
        private XDocument ApplyGenerateMode(
            XDocument doc,
            XElement originalElem,
            string templateXml,
            IEnumerable<IDictionary<string, string>> excelRows)
        {
            var replacementNodes = new List<object>();

            foreach (var row in excelRows)
            {
                var filled = _templateService.ApplyPlaceholders(templateXml, row);
                var newElem = XElement.Parse(filled, LoadOptions.PreserveWhitespace);

                // Keep layout reasonably stable with newline + two spaces
                replacementNodes.Add(new XText(Environment.NewLine + "  "));
                replacementNodes.Add(newElem);
            }

            // Trailing newline padding for nicer diffs
            replacementNodes.Add(new XText(Environment.NewLine + "  "));

            originalElem.ReplaceWith(replacementNodes.ToArray());
            return doc;
        }

        // ==========================================================
        // UPDATE MODE: find existing children and update matching ones
        // ==========================================================
        /// <summary>
        /// Finds existing child elements (by <see cref="XMLKeyDefinition"/> rules) and
        /// replaces matching ones with newly generated elements from templateXml.
        /// Non-matching rows are skipped (no inserts by default).
        /// </summary>
        private XDocument ApplyUpdateMode(
            XDocument doc,
            XElement selectedElem,
            string templateXml,
            IEnumerable<IDictionary<string, string>> excelRows,
            XMLKeyDefinition keyDef)
        {
            // Parent contains all instances we want to edit
            var parent = selectedElem.Parent ?? throw new InvalidOperationException("Selected element has no parent.");

            // Take a snapshot of children of that same name (e.g. all <group>)
            var candidates = parent.Elements()
                                   .Where(e => e.Name.LocalName == keyDef.ElementName)
                                   .ToList();

            foreach (var row in excelRows)
            {
                // Excel key
                if (!row.TryGetValue("key", out var excelKey) || string.IsNullOrEmpty(excelKey))
                    continue;

                // find corresponding existing xml element
                var match = candidates.FirstOrDefault(e =>
                {
                    var attr = e.Attribute(keyDef.AttributeName);
                    return attr != null && string.Equals(attr.Value, excelKey, StringComparison.OrdinalIgnoreCase);
                });

                if (match == null)
                {
                    // optional: if not found, we could create new here
                    continue;
                }

                // We have the existing element, now we need to apply the other placeholders.
                // Easiest way (for now): re-generate full element from template,
                // but KEEP the position by ReplaceWith
                var filled = _templateService.ApplyPlaceholders(templateXml, row);
                var newElem = XElement.Parse(filled, LoadOptions.PreserveWhitespace);

                // replace IN PLACE
                match.ReplaceWith(new XText(Environment.NewLine + "  "), newElem);
            }

            return doc;
        }

        // ==========================================================
        // Helper: path-based lookup used to map UI selection to XElement
        // Path format: /TagName[index] where index is sibling index among same-name elements
        // ==========================================================
        private XElement? FindByGeneratedId(XElement element, string targetId, string parentPath)
        {
            var index = 0;
            if (element.Parent != null)
            {
                var same = element.Parent.Elements(element.Name).ToList();
                index = same.IndexOf(element);
            }

            var currentPath = $"{parentPath}/{element.Name.LocalName}[{index}]";

            if (currentPath == targetId)
                return element;

            foreach (var child in element.Elements())
            {
                var found = FindByGeneratedId(child, targetId, currentPath);
                if (found != null)
                    return found;
            }

            return null;
        }

    }
}
