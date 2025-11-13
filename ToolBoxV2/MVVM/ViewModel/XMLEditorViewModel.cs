using DocumentFormat.OpenXml.Spreadsheet;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Application.LocalMessages;
using ToolBoxV2.Application.XMLEditor;
using ToolBoxV2.Domain.XMLEditor;
using ToolBoxV2.Infrastracture.Common;
using ToolBoxV2.Infrastracture.XMLEditor;
using ToolBoxV2.Presentation.WPF.Core;
using ToolBoxV2.Presentation.WPF.Services;
using static ToolBoxV2.Application.XMLEditor.IXMLNodeEditService;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{
    public class XMLEditorViewModel : ObservableObject
    {
        private string _xmlEditTargetPath = Properties.Settings.Default.XMLEditTargetPath;
        public string XMLEditTargetPath
        {
            get => _xmlEditTargetPath;
            set
            {
                if (SetProperty(ref _xmlEditTargetPath, value))
                {
                    Properties.Settings.Default.XMLEditTargetPath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private string _xmlEditFileExPath = Properties.Settings.Default.XMLEditFileExPath;
        public string XMLEditFileExPath
        {
            get => _xmlEditFileExPath;
            set
            {
                if (SetProperty(ref _xmlEditFileExPath, value))
                {
                    Properties.Settings.Default.XMLEditFileExPath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private string _xmlEditFileXmlPath = Properties.Settings.Default.XMLEditFileXmlPath;
        public string XMLEditFileXmlPath
        {
            get => _xmlEditFileXmlPath;
            set
            {
                if (SetProperty(ref _xmlEditFileXmlPath, value))
                {
                    Properties.Settings.Default.XMLEditFileXmlPath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private string _xmlEditSheetName = Properties.Settings.Default.XMLEditSheetName;
        public string XMLEditSheetName
        {
            get => _xmlEditSheetName;
            set
            {
                if (SetProperty(ref _xmlEditSheetName, value))
                {
                    Properties.Settings.Default.XMLEditSheetName = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private int _xmlEditHeaderRow = Properties.Settings.Default.XMLEditHeaderRow;
        public int XMLEditHeaderRow
        {
            get => _xmlEditHeaderRow;
            set
            {
                if (SetProperty(ref _xmlEditHeaderRow, value))
                {
                    Properties.Settings.Default.XMLEditHeaderRow = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    // When loading state changes, tell WPF to re-query CanExecute
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private XMLNodeModel _rootNode;
        public XMLNodeModel RootNode
        {
            get => _rootNode;
            set => SetProperty(ref _rootNode, value);
        }

        // Currently selected XML node (populated when user clicks TreeView)
        private XMLBlock _selectedBlock;
        public XMLBlock SelectedBlock
        {
            get => _selectedBlock;
            set => SetProperty(ref _selectedBlock, value);
        }

        // create the table once and KEEP it
        private readonly DataTable _table = new DataTable();
        private DataView _tableView;
        public DataView TableView
        {
            get => _tableView;
            private set => SetProperty(ref _tableView, value);
        }

        private readonly IExcelReader _excelReader;
        private readonly IDiagnosticLogger _logger;
        private readonly IXMLReaderService _xmlReaderService;

        //==============================================================================================================================================================================================================

        // Holds the XML currently being edited in the TextBox
        private string _templateXml;
        public string TemplateXml
        {
            get => _templateXml;
            set => SetProperty(ref _templateXml, value);
        }
        // Parameter selected from the right-side ListBox (e.g. "groupname", "value" ...)
        private string _selectedParameter;
        public string SelectedParameter
        {
            get => _selectedParameter;
            set => SetProperty(ref _selectedParameter, value);
        }

        // Parameter selected from the right-side ListBox (e.g. "groupname", "value" ...)
        private int _selectionStart;
        public int SelectionStart
        {
            get => _selectionStart;
            set => SetProperty(ref _selectionStart, value);
        }

        // The length (number of characters) of the user’s selection in the TextBox
        private int _selectionLength;
        public int SelectionLength
        {
            get => _selectionLength;
            set => SetProperty(ref _selectionLength, value);
        }

        // List of available parameters the user can insert
        public ObservableCollection<string> Parameters { get; set; } = new();

        public ICommand ReplaceSelectionCommand { get; }


        private string _generatedXmlPreview;
        public string GeneratedXmlPreview
        {
            get => _generatedXmlPreview;
            set => SetProperty(ref _generatedXmlPreview, value);
        }

        public ICommand GenerateCommand { get; }
        public ICommand SaveGeneratedCommand { get; }

        private readonly IXMLNodeEditService _xmlNodeEditService;        
        private readonly IXMLExportService _xmlExportService;

        // which operation to perform
        private XMLEditMode _editMode = XMLEditMode.Generate;
        public XMLEditMode EditMode
        {
            get => _editMode;
            set => SetProperty(ref _editMode, value);
        }

        // which attribute on the target element is the key (e.g., "name", "id")
        private string _keyAttributeName = "name";
        public string KeyAttributeName
        {
            get => _keyAttributeName;
            set => SetProperty(ref _keyAttributeName, value);
        }

        private string _selectedKeyColumn;
        public string SelectedKeyColumn
        {
            get => _selectedKeyColumn;
            set => SetProperty(ref _selectedKeyColumn, value);
        }

        public Array EditModes => Enum.GetValues(typeof(XMLEditMode));
        private readonly IFileDialogService _fileDialogService;

        //==============================================================================================================================================================================================================

        public ICommand ImportCommand { get; }
        public ICommand BrowseExcelCommand { get; }
        public ICommand BrowseXmlCommand { get; }
        public ICommand BrowseFolderCommand { get; }

        public XMLEditorViewModel(IExcelReader excelReader, IDiagnosticLogger logger, IXMLReaderService xmlReaderService, IXMLNodeEditService xmlNodeEditService, IXMLExportService xmlExportService, IFileDialogService fileDialogService)
        {
            _excelReader = excelReader;
            _logger = logger;
            _xmlReaderService = xmlReaderService;
            _xmlNodeEditService = xmlNodeEditService;            
            _xmlExportService = xmlExportService;
            _fileDialogService = fileDialogService;

            ReplaceSelectionCommand = new RelayCommand(_ => ReplaceSelection());
            GenerateCommand = new RelayCommand(async _ => await GenerateFromTemplateAsync());
            SaveGeneratedCommand = new RelayCommand(_ => SaveGeneratedXml());
            BrowseExcelCommand = new RelayCommand(async _ => await BrowseAndAssignAsync(FileBrowseTarget.ExcelFile, p => XMLEditFileExPath = p));
            BrowseXmlCommand = new RelayCommand(async _ => await BrowseAndAssignAsync(FileBrowseTarget.XmlFile, p => XMLEditFileXmlPath = p));
            BrowseFolderCommand = new RelayCommand(async _ => await BrowseAndAssignAsync(FileBrowseTarget.Folder, p => XMLEditTargetPath = p));


            ImportCommand = new RelayCommand(async _ =>
            {
                var excelTask = LoadExcelAsync();
                var xmlTask = LoadXmlAsync();

                await Task.WhenAll(excelTask, xmlTask);
            });
        }

        private async Task LoadExcelAsync()
        {
            if (IsLoading)
                return; // just in case someone bypasses UI
            try
            {
                IsLoading = true;
                if (string.IsNullOrWhiteSpace(XMLEditSheetName) || string.IsNullOrWhiteSpace(XMLEditFileXmlPath) || string.IsNullOrWhiteSpace(XMLEditFileExPath))
                {
                    _logger.Warn("File path or sheet name is empty.");
                    return;
                }

                // clear previous data but DON'T replace the DataTable instance
                _table.Clear();
                _table.Columns.Clear();

                var req = new ExcelReadRequest
                {
                    FilePath = XMLEditFileExPath,
                    SheetName = XMLEditSheetName,
                    HeaderRowIndex = XMLEditHeaderRow,
                    ExpectedColumns = Array.Empty<string>()
                };
                int counter = 0;

                bool columnsCreated = false;

                await foreach (var row in _excelReader.ReadAsync(req))
                {
                    if (!columnsCreated)
                    {
                        foreach (var colName in row.Cells.Keys)
                        {
                            _table.Columns.Add(colName);
                        }

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Parameters.Clear();
                            foreach (var colName in _table.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
                                Parameters.Add(colName);
                        });

                        columnsCreated = true;

                        // Create a NEW DataView so WPF regenerates columns
                        TableView = _table.DefaultView;
                    }

                    var dr = _table.NewRow();
                    foreach (var kvp in row.Cells)
                    {
                        dr[kvp.Key] = kvp.Value ?? DBNull.Value;
                    }
                    _table.Rows.Add(dr);

                    counter++;
                    if (counter % 10 == 0)
                    {
                        await Task.Delay(30);
                    }
                }               
              
                _logger.Info($"Imported {_table.Rows.Count} Excel Rows.");
            }
            catch (Exception ex)
            {
                _logger.Error("Error occured during importing. ", ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // called from UI after user chooses a file
        private async Task LoadXmlAsync()
        {
            if (string.IsNullOrWhiteSpace(XMLEditFileXmlPath))
                return;

            RootNode = await Task.Run(() => _xmlReaderService.LoadXmlAsTree(XMLEditFileXmlPath));
        }



        //==============================================================================================================================================================================================================

        
        // called from code-behind when a TreeView node is selected
        public void OnNodeSelected(XMLNodeModel node)
        {
            // Ask the XML service for that node’s raw XML block
            SelectedBlock = _xmlReaderService.GetBlockById(XMLEditFileXmlPath, node.Id);
            // Show the block in the editor
            TemplateXml = SelectedBlock.RawXml;

            // Reset selection info
            SelectionStart = 0;
            SelectionLength = 0;
        }

        private void ReplaceSelection()
        {
            // must have a param
            if (string.IsNullOrWhiteSpace(SelectedParameter))
                return;

            // must have text
            if (string.IsNullOrEmpty(TemplateXml))
                return;

            // must have some selection
            if (SelectionLength <= 0)
                return;

            // Split the text into parts: before selection, and after selection
            var before = TemplateXml.Substring(0, SelectionStart);
            var after = TemplateXml.Substring(SelectionStart + SelectionLength);

            // Build placeholder, e.g. [groupname]
            var placeholder = $"[{SelectedParameter}]";

            // Recombine text with inserted placeholder
            TemplateXml = before + placeholder + after;

            // Optionally move caret just after inserted placeholder
            SelectionStart = (before + placeholder).Length;
            SelectionLength = 0;
        }


        private async Task GenerateFromTemplateAsync()
        {
            if (string.IsNullOrWhiteSpace(TemplateXml))
            {
                _logger.Warn("No template XML to generate from.");
                return;
            }

            if (SelectedBlock == null)
            {
                _logger.Warn("No XML node selected.");
                return;
            }

            if (string.IsNullOrWhiteSpace(XMLEditFileXmlPath) || !File.Exists(XMLEditFileXmlPath))
            {
                _logger.Warn("XML source file not found.");
                return;
            }

            // collect excel rows from your DataTable/DataView into a simpler structure
            var excelRows = CollectExcelRows();
            // Build key definition only for Update mode
            XMLKeyDefinition? keyDef = null;
            if (EditMode == XMLEditMode.Update)
            {                           
                if (string.IsNullOrWhiteSpace(SelectedKeyColumn))
                { _logger.Warn("Key column name is empty."); return; }

                // Optional sanity check: first row contains the key column?
                if (excelRows.Any() && !excelRows.First().ContainsKey(SelectedKeyColumn))
                {
                    _logger.Warn($"Excel rows do not contain key column '{SelectedKeyColumn}'.");
                    return;
                }

                var placeholder = $"[{SelectedKeyColumn}]";

                XElement root;
                try
                {
                    root = XElement.Parse(TemplateXml);
                }
                catch
                {
                    _logger.Warn("Template XML is not a valid element.");
                    return;
                }

                // Look for the attribute that contains the placeholder
                var matches = root
                    .DescendantsAndSelf() // include root + all children
                    .SelectMany(e => e.Attributes(), (elem, attr) => new { elem, attr })
                    .Where(x =>
                        (x.attr.Value ?? string.Empty)
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Trim()
                            .Contains(placeholder, StringComparison.Ordinal))
                    .ToList();

                if (matches.Count == 0)
                {
                    _logger.Warn($"Placeholder '{placeholder}' not found in template. Key must appear exactly once.");
                    return;
                }

                if (matches.Count > 1)
                {
                    _logger.Warn($"Placeholder '{placeholder}' appears {matches.Count} times in template. Key must appear exactly once.");
                    return;
                }

                var match = matches[0];

                keyDef = new XMLKeyDefinition
                {
                    ElementName = match.elem.Name.LocalName,
                    AttributeName = match.attr.Name.LocalName,
                    KeyColumnName = SelectedKeyColumn
                };
            }

            var finalXml = await Task.Run(() =>
            {
                // load document here in VM or (even better) inside the service — both ok for now
                var doc = XDocument.Load(XMLEditFileXmlPath, LoadOptions.PreserveWhitespace);

                // call application service to do the heavy XML work
                var updatedDoc = _xmlNodeEditService.Apply(
                doc,
                SelectedBlock.NodeId,
                TemplateXml,
                excelRows,
                EditMode,
                keyDef
                );

                /*
                // return as string
                using var sw = new StringWriterWithEncoding(Encoding.UTF8);
                // keep declaration if service didn’t set it
                if (updatedDoc.Declaration == null)
                    updatedDoc.Declaration = new XDeclaration("1.0", "utf-8", null);
                updatedDoc.Save(sw, SaveOptions.DisableFormatting);
                return sw.ToString();*/

                // For preview we don't need infra helpers; ToString is fine.
                if (updatedDoc.Declaration == null)
                    updatedDoc.Declaration = new XDeclaration("1.0", "utf-8", null);

                return updatedDoc.ToString(SaveOptions.DisableFormatting);
            });

            GeneratedXmlPreview = finalXml;
            _logger.Info("XML generated from template (via service).");
        }

        // helper: turn your DataView into IEnumerable<Dictionary<string,string>>
        private IEnumerable<Dictionary<string, string>> CollectExcelRows()
        {
            var list = new List<Dictionary<string, string>>();

            if (TableView == null)
                return list;

            foreach (DataRowView rowView in TableView)
            {
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataColumn col in rowView.Row.Table.Columns)
                {
                    var valObj = rowView.Row[col];
                    var val = valObj == DBNull.Value ? string.Empty : valObj?.ToString() ?? string.Empty;
                    dict[col.ColumnName] = val;
                }
                list.Add(dict);
            }

            return list;
        }

        private void SaveGeneratedXml()
        {
            if (string.IsNullOrWhiteSpace(GeneratedXmlPreview))
            {
                _logger.Warn("No generated XML to save. Click Generate first.");
                return;
            }

            if (string.IsNullOrWhiteSpace(XMLEditTargetPath))
            {
                _logger.Warn("Target path is empty.");
                return;
            }

            try
            {
                // parse the preview back to XDocument
                var doc = XDocument.Parse(GeneratedXmlPreview, LoadOptions.PreserveWhitespace);

                var baseName = Path.GetFileNameWithoutExtension(XMLEditFileXmlPath) + "_generated";

                // delegate to service
                _xmlExportService.Save(doc, XMLEditTargetPath, baseName);

                _logger.Info("Generated XML saved via export service.");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save generated XML.", ex);
            }
        }

        private async Task BrowseAndAssignAsync(FileBrowseTarget target, Action<string> assignPath)
        {
            var path = await _fileDialogService.BrowseAsync(target);

            if (!string.IsNullOrWhiteSpace(path))
                assignPath(path);
        }

    }
}
