using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Presentation.WPF.Services
{
    public enum FileBrowseTarget
    {
        ExcelFile,
        XmlFile,
        Folder
    }
    public interface IFileDialogService
    {
        Task<string?> BrowseAsync(FileBrowseTarget target);
    }
}
