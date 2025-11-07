using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Application.LocalMessages;
using ToolBoxV2.Presentation.WPF.Core;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{
    public class XMLEditorViewModel : ObservableObject
    {
        private string _xmlEditTargetPath;
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

        private string _xmlEditFileExPath;
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

        private string _xmlEditFileXmlPath;
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

        private string _xmlEditSheetName;
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

        private int _xmlEditHeaderRow;
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

        public ICommand ImportCommand { get; }

        public XMLEditorViewModel(IExcelReader excelReader, IDiagnosticLogger logger)
        {
            _excelReader = excelReader;
            _logger = logger;


            XMLEditSheetName = "Sheet1";
            XMLEditFileExPath = "C:\\Users\\Eng\\Desktop\\ToolBoxV2/ExcelTest.xlsx";
            XMLEditFileXmlPath = "C:\\Users\\Eng\\Desktop\\ToolBoxV2/ExcelTest.xlsx";
            XMLEditTargetPath = "C:\\Users\\Eng\\Desktop\\ToolBoxV2\\TestFolder";
            

            ImportCommand = new RelayCommand(async _ => await LoadAsync());
        }

        private async Task LoadAsync()
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
                    HeaderRowIndex = 1,
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
    }    
}
