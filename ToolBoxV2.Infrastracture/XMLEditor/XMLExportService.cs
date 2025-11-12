using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Infrastracture.Common;

namespace ToolBoxV2.Application.XMLEditor
{
    /// <summary>
    /// Concrete implementation of <see cref="IXMLExportService"/> that saves XML
    /// documents to the local file system using UTF-8 encoding.
    ///
    /// Layer & role:
    /// Infrastructure layer — this class bridges the Application-level
    /// abstraction (<see cref="IXMLExportService"/>) with actual IO logic.
    ///     
    /// Usage:
    /// Typically invoked by a ViewModel through dependency injection of
    /// the <see cref="IXMLExportService"/> interface when the user triggers a
    /// "Save" or "Export" action.
    /// </summary>
    
    public class XMLExportService : IXMLExportService
    {
        public void Save(XDocument document, string targetDirectory, string baseName)
        {
            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            var filePath = Path.Combine(targetDirectory, baseName + ".xml");

            // preserve or create declaration
            if (document.Declaration == null)
                document.Declaration = new XDeclaration("1.0", "utf-8", null);

            // Use a custom StringWriter that enforces UTF-8 encoding
            using var sw = new StringWriterWithEncoding(Encoding.UTF8);
            // Save without formatting to preserve exact node layout
            document.Save(sw, SaveOptions.DisableFormatting);
            // Write text using UTF-8 without BOM
            File.WriteAllText(filePath, sw.ToString(), new UTF8Encoding(false));
        }
    }
}
