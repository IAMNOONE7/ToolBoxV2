using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Application.LocalMessages;
using ToolBoxV2.Domain.LocalMessages;
using ToolBoxV2.Presentation.WPF.Core;
using ToolBoxV2.Presentation.WPF.Properties;
using ToolBoxV2.Presentation.WPF.Services;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{
    public class LocalMessagesViewModel : ObservableObject
    {
        private string _localMessTargetPath = Properties.Settings.Default.LocalMessTargetPath;
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

        private string _localMessFilePath = Properties.Settings.Default.LocalMessFilePath;
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

        private string _localMessSheetName = Properties.Settings.Default.LocalMessSheetName;
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

        private int _localMessHeaderRow = Properties.Settings.Default.LocalMessHeaderRow;
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

        private readonly IFileDialogService _fileDialogService;
        private readonly ILocalMessageExportService _messageExportService;
        public ICommand BrowseExcelCommand { get; }        
        public ICommand BrowseFolderCommand { get; }

        public ICommand GenerateLocFilesCommand { get; }

        public LocalMessagesViewModel(IExcelReader excelReader, IDiagnosticLogger logger, ILocalMessageIncrementalBuilderFactory builderFactory, IFileDialogService fileDialogService, ILocalMessageExportService messageExportService)
        {
            _excelReader = excelReader;
            _logger = logger;
            _builderFactory = builderFactory;
            _fileDialogService = fileDialogService;
            _messageExportService = messageExportService;


            ImportCommand = new RelayCommand(async _ => await ImportAsync(), _ => !IsLoading);
            BrowseExcelCommand = new RelayCommand(async _ => await BrowseAndAssignAsync(FileBrowseTarget.ExcelFile, p => LocalMessFilePath = p));            
            BrowseFolderCommand = new RelayCommand(async _ => await BrowseAndAssignAsync(FileBrowseTarget.Folder, p => LocalMessTargetPath = p));
            GenerateLocFilesCommand = new RelayCommand(async _ => await GenerateLocFilesAsync());
        }      

        private async Task ImportAsync()
        {
            //==========================================
            // TODO: cancelation Token
            //==========================================
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

        private async Task BrowseAndAssignAsync(FileBrowseTarget target, Action<string> assignPath)
        {
            var path = await _fileDialogService.BrowseAsync(target);

            if (!string.IsNullOrWhiteSpace(path))
                assignPath(path);
        }

        private async Task GenerateLocFilesAsync()
        {
            if (string.IsNullOrWhiteSpace(LocalMessTargetPath) || !Directory.Exists(LocalMessTargetPath))
            {
                _logger.Warn("Target path is not valid.");
                return;
            }

            if (LM == null || LM.Count == 0)
            {
                _logger.Warn("No Local Messages to export.");
                return;
            }           

            try
            {
                var result = await _messageExportService.ExportAsync(LocalMessTargetPath, LM);

                if (result.FailureCount > 0 && result.SuccessCount == 0)
                {
                    _logger.Warn("Local Message generation failed.");
                }
                else if (result.FailureCount > 0)
                {
                    _logger.Warn(
                        $"Local Message generation completed with issues. Success: {result.SuccessCount}, Failed: {result.FailureCount}.");
                }
                else
                {
                    _logger.Info($"Local Message generation completed. Files: {result.SuccessCount}.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Unexpected error during Local Message generation.", ex);
            }
            finally
            {
                
            }
        }
    }
}
