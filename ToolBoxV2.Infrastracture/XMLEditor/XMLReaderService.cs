using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Application.XMLEditor;
using ToolBoxV2.Domain.XMLEditor;

namespace ToolBoxV2.Infrastracture.XMLEditor
{
    /// <summary>
    /// Concrete implementation of <see cref="IXMLReaderService"/> that loads XML
    /// documents from disk and converts them into structured models.
    ///
    /// Layer role:
    /// Infrastructure layer — provides the concrete "how" for XML parsing,
    /// while the Application layer defines the *what* (via <see cref="IXMLReaderService"/>). 
    /// </summary>
    public class XMLReaderService : IXMLReaderService
    {
        /// <summary>
        /// Loads the given XML file and converts it into a recursive <see cref="XMLNodeModel"/>
        /// tree, preserving element order and whitespace.
        /// </summary>        
        /// returns The root <see cref="XMLNodeModel"/> representing the XML structure.
        
        public XMLNodeModel LoadXmlAsTree(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML file not found", filePath);

            // PreserveWhitespace is important for these HMI-like XMLs
            var doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
            var root = doc.Root ?? throw new InvalidOperationException("XML has no root element.");

            return ConvertToModel(root, "");
        }

        /// <summary>
        /// Extracts a specific XML node by its generated ID and returns it as a raw block.
        /// 
        /// Why this method re-reads the file:
        /// The XML is read again instead of being reused from memory to ensure:        
        ///   Freshness – the file might have been edited or generated externally since last load.
        ///   Isolation – avoids keeping large XDocument trees alive in memory for the whole session.
        ///   Simplicity – prevents accidental side effects if other parts of the app hold a modified in-memory copy.        
        /// In practice, reloading is inexpensive for small-to-medium XMLs (HMI screens, alarms, etc.)
        /// and guarantees consistency between the UI tree and file content.        
        /// returns
        /// An <see cref="XMLBlock"/> containing the node ID and its raw, compact XML markup.
        

        public XMLBlock GetBlockById(string filePath, string nodeId)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML file not found", filePath);

            // Load fresh copy from disk — see comment above for reasoning.
            var doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
            var root = doc.Root ?? throw new InvalidOperationException("XML has no root element.");

            var element = FindByGeneratedId(root, nodeId, "");
            if (element == null)
                throw new InvalidOperationException($"Node with id '{nodeId}' not found in XML.");

            // DisableFormatting - we get a compact version that we can safely string-replace
            return new XMLBlock
            {
                NodeId = nodeId,
                RawXml = element.ToString(SaveOptions.DisableFormatting)
            };
        }
        // ==========================================================
        // Recursive conversion from XElement -> XMLNodeModel (Domain)
        // ==========================================================
        private XMLNodeModel ConvertToModel(XElement element, string parentPath)
        {
            // Generate a stable unique ID using sibling index within same-named elements
            var index = GetIndexAmongSameNamedSiblings(element);
            var currentPath = $"{parentPath}/{element.Name.LocalName}[{index}]";

            var model = new XMLNodeModel
            {
                Id = currentPath,
                Name = element.Name.LocalName,
            };

            // attributes
            foreach (var attr in element.Attributes())
            {                
                model.Attributes[attr.Name.LocalName] = attr.Value;
            }

            // if the element has only text inside (no child elements) we can show it
            // e.g. <text ... caption="..."/> is already in attributes
            // but some FT XMLs may have <param>value</param>
            var hasElementChildren = element.Elements().Any();
            if (!hasElementChildren)
            {
                // value can be whitespace in HMI XMLs, so trim carefully
                var text = element.Value?.Trim();
                if (!string.IsNullOrEmpty(text))
                    model.TextContent = text;
            }

            // children (in order!)
            foreach (var child in element.Elements())
            {
                var childModel = ConvertToModel(child, currentPath);
                model.Children.Add(childModel);
            }

            return model;
        }

        // ==========================================================
        // Helper: find element by generated path-based ID
        // ==========================================================

        private XElement? FindByGeneratedId(XElement element, string targetId, string parentPath)
        {
            var index = GetIndexAmongSameNamedSiblings(element);
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
        // ==========================================================
        // Helper: get sibling index among same-named elements
        // ==========================================================
        private int GetIndexAmongSameNamedSiblings(XElement element)
        {
            if (element.Parent == null)
                return 0;

            var sameNameSiblings = element.Parent
                .Elements(element.Name)
                .ToList();

            return sameNameSiblings.IndexOf(element);
        }
    }
}
