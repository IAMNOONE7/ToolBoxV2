using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Application.LocalMessages;
using ToolBoxV2.Domain.LocalMessages;
using ToolBoxV2.Presentation.WPF.Core;
using ToolBoxV2.Presentation.WPF.Properties;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{
    public class LocalMessagesViewModel : ObservableObject
    {
        private string _localMessTargetPath;
        public string LocalMessTargetPath
        {
            get => _localMessTargetPath;
            set
            {
                if (SetProperty(ref _localMessTargetPath, value))
                {
                    Properties.Settings.Default.LocalMessTargetPath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private string _localMessFilePath;
        public string LocalMessFilePath
        {
            get => _localMessFilePath;
            set
            {
                if (SetProperty(ref _localMessFilePath, value))
                {
                    Properties.Settings.Default.LocalMessFilePath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private string _localMessSheetName;
        public string LocalMessSheetName
        {
            get => _localMessSheetName;
            set
            {
                if (SetProperty(ref _localMessSheetName, value))
                {
                    Properties.Settings.Default.LocalMessSheetName = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private int _localMessHeaderRow;
        public int LocalMessHeaderRow
        {
            get => _localMessHeaderRow;
            set
            {
                if (SetProperty(ref _localMessHeaderRow, value))
                {
                    Properties.Settings.Default.LocalMessHeaderRow = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private LocalMessage? _selectedLocalMessage;
        public LocalMessage? SelectedLocalMessage
        {
            get => _selectedLocalMessage;
            set
            {
                if (SetProperty(ref _selectedLocalMessage, value))
                {
                    Items.Clear();
                    if (value != null)
                    {
                        foreach (var it in value.Items)
                            Items.Add(it);
                    }
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

        private readonly IExcelReader _excelReader;
        private readonly IDiagnosticLogger _logger;
        private readonly ILocalMessageIncrementalBuilderFactory _builderFactory;

        public ObservableCollection<LocalMessage> LM { get; } = new();
        public ObservableCollection<LocalMessageItem> Items { get; } = new();
        public ICommand ImportCommand { get; }

        public LocalMessagesViewModel(IExcelReader excelReader, IDiagnosticLogger logger, ILocalMessageIncrementalBuilderFactory builderFactory)
        {
            _excelReader = excelReader;
            _logger = logger;
            _builderFactory = builderFactory;

            //LocalMessSheetName = Properties.Settings.Default.LocalMessSheetName;
            //LocalMessFilePath = Properties.Settings.Default.LocalMessFilePath;
            //LocalMessTargetPath = Properties.Settings.Default.LocalMessTargetPath;

            LocalMessSheetName = "List1";
            LocalMessFilePath = "C:\\Users\\Eng\\Desktop\\ToolBoxV2/ExcelTest.xlsx";
            LocalMessTargetPath = "C:\\Users\\Eng\\Desktop\\ToolBoxV2\\TestFolder";
            ImportCommand = new RelayCommand(async _ => await ImportAsync(), _ => !IsLoading);
        }

        private void SelectFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Excel File",
                Filter = "Excel Files (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                LocalMessFilePath = dialog.FileName;
                _logger.Info($"Selected file: {LocalMessFilePath}");                   
            }
        }

        private void SelectTargetPath()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Target Path"
            };

            if (dialog.ShowDialog() == true)
            {
                LocalMessTargetPath = dialog.FolderName;
                _logger.Info($"Selected Target Path: {LocalMessTargetPath}");                
            }
        }

        private async Task ImportAsync()
        {
            if (IsLoading)
                return; // just in case someone bypasses UI
            try
            {
                IsLoading = true;
                if (string.IsNullOrWhiteSpace(LocalMessFilePath) || string.IsNullOrWhiteSpace(LocalMessSheetName))
                {
                    _logger.Warn("File path or sheet name is empty.");
                    return;
                }
                LM.Clear();
                Items.Clear();
                SelectedLocalMessage = null;

                var req = new ExcelReadRequest
                {
                    FilePath = LocalMessFilePath,
                    SheetName = LocalMessSheetName,
                    HeaderRowIndex = 1,
                    ExpectedColumns = new[] { "Name", "Index", "Text" }
                };
                int counter = 0;
                // --- create a fresh builder for this import ---
                var _builder = _builderFactory.Create();
                // ------------------------------------------------
                await foreach (var row in _excelReader.ReadAsync(req))
                {
                    var update = _builder.ApplyRow(row);

                    switch (update.Kind)
                    {
                        case LocalMessageUpdateKind.NewMessage:
                            LM.Add(update.Message!);
                            SelectedLocalMessage = update.Message!;
                            break;
                        case LocalMessageUpdateKind.NewItem:
                            if (SelectedLocalMessage == update.Message && update.Item != null)
                                Items.Add(update.Item);
                            break;
                    }

                    counter++;
                    if (counter % 10 == 0)
                    {
                        await Task.Delay(30);
                    }
                }

                _logger.Info($"Imported {_builder.GetAll().Count} local messages.");
            }
            catch(Exception ex) 
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
