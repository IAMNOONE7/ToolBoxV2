using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Application.XMLEditor
{
    // Provides functionality to replace placeholders in XML template strings
    // with concrete values from a data source (such as Excel rows).
    //
    // Implemented by:
    // Infrastructure service (XMLTemplateService) that handles
    // actual string manipulation 
    public interface IXMLTemplateService
    {
        
        // Replaces placeholders like [ColumnName] in the template with values from the row.
        // Missing values -> empty string.       
        string ApplyPlaceholders(string templateXml, IDictionary<string, string> values);
    }
}
