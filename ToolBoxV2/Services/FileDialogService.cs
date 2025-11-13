using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToolBoxV2.Presentation.WPF.Services
{
    public class FileDialogService : IFileDialogService
    {
        public Task<string?> BrowseAsync(FileBrowseTarget target)
        {
            switch (target)
            {
                case FileBrowseTarget.ExcelFile:
                    return Task.FromResult(
                        ShowOpenFileDialog("Excel files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm"));

                case FileBrowseTarget.XmlFile:
                    return Task.FromResult(
                        ShowOpenFileDialog("XML files (*.xml)|*.xml"));

                case FileBrowseTarget.Folder:
                    return Task.FromResult(
                        ShowFolderDialog());

                default:
                    return Task.FromResult<string?>(null);
            }
        }

        private string? ShowOpenFileDialog(string filter)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                CheckFileExists = true,
                Multiselect = false
            };

            var result = dialog.ShowDialog();
            return result == DialogResult.OK ? dialog.FileName : null;
        }

        private string? ShowFolderDialog()
        {
            using var dialog = new FolderBrowserDialog
            {
                ShowNewFolderButton = true
            };

            var result = dialog.ShowDialog();
            return result == DialogResult.OK ? dialog.SelectedPath : null;
        }
    }
}
