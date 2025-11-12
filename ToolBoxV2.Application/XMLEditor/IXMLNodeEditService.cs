using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ToolBoxV2.Domain.XMLEditor;

namespace ToolBoxV2.Application.XMLEditor
{
    
    // Provides logic for modifying XML documents based on tabular (Excel) data.
    //
    // Application interface — defines high-level edit semantics
    // for inserting or updating XML nodes using provided key definitions.
    // 
    // The ViewModel (UI) knows when to edit, but this service knows how.
    //
    // Implemented by:
    // Infrastructure service (XMLNodeEditService) that manipulates
    // the XDocument structure directly.
   

    public interface IXMLNodeEditService
    {
        // Defines how Excel data should be applied to the XML document.
        public enum XMLEditMode
        {
            Generate,   //create N new nodes
            Update      //find existing nodes by key and update them
        }

        // Applies data rows from Excel to a selected node within the XML document,
        // either by generating new nodes or updating existing ones.


        XDocument Apply(
            XDocument sourceDocument,
            string selectedNodeId,
            string templateXml,
            IEnumerable<IDictionary<string, string>> excelRows,
            XMLEditMode mode,
            XMLKeyDefinition keyDef = null);
    }
}
