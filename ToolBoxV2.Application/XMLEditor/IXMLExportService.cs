using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ToolBoxV2.Application.XMLEditor
{
    
    // Defines how an XML document should be persisted to disk.    
    // Application layer interface — describes the operation
    // of saving a generated or edited XML document without caring
    // about the concrete file system implementation.
    
    // Implemented by:
    // Infrastructure service (XMLExportService) that
    // performs the actual file IO using .NET or other libraries.
    
    public interface IXMLExportService
    {
        void Save(XDocument document, string targetDirectory, string baseName);
    }
}
