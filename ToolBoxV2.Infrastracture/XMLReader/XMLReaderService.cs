using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Application.XMLEditor;
using ToolBoxV2.Domain.XMLEditor;

namespace ToolBoxV2.Infrastracture.XMLReader
{
    public class XMLReaderService : IXMLReaderService
    {
        public XMLNodeModel LoadXmlAsTree(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML file not found", filePath);

            // PreserveWhitespace is important for these HMI-like XMLs
            var doc = XDocument.Load(filePath, LoadOptions.PreserveWhitespace);
            var root = doc.Root ?? throw new InvalidOperationException("XML has no root element.");

            return ConvertToModel(root, "");
        }

        public XMLBlock GetBlockById(string filePath, string nodeId)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("XML file not found", filePath);

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

        private XMLNodeModel ConvertToModel(XElement element, string parentPath)
        {
            // We make ids stable by index among siblings with the same name
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
