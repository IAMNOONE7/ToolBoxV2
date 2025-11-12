using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Domain.XMLEditor;

namespace ToolBoxV2.Application.XMLEditor
{    
    // Abstraction for reading and interpreting XML files into
    // structured domain objects usable by the UI or business logic.
    //    
    // Application interface — defines read operations
    // without committing to a specific XML library or file access method.
    // 
    // Implemented by:
    // Infrastructure service (XMLReaderService) that performs actual XML parsing.
    
    
    public interface IXMLReaderService
    {
        // load file and build domain tree
        XMLNodeModel LoadXmlAsTree(string filePath);

        // get single node's raw xml (for cloning/edit)
        XMLBlock GetBlockById(string filePath, string nodeId);
    
    }
}
