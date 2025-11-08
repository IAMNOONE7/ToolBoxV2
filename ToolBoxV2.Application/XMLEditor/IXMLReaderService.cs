using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Domain.XMLEditor;

namespace ToolBoxV2.Application.XMLEditor
{
    public interface IXMLReaderService
    {
        // load file and build domain tree
        XMLNodeModel LoadXmlAsTree(string filePath);

        // get single node's raw xml (for cloning/edit)
        XMLBlock GetBlockById(string filePath, string nodeId);
    
    }
}
