using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ToolBoxV2.Presentation.WPF.Core;
using ToolBoxV2.Presentation.WPF.Services.Diagnostics;
using ToolBoxV2.Presentation.WPF.Services.SnackBar;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private readonly ISnackBarManager _snackBarManager;
        public MessageToSnack CurrentMessage => _snackBarManager.CurrentMessage;
        private Brush _snackbarBackgroundBrush;
        public Brush SnackbarBackgroundBrush
        {
            get => _snackbarBackgroundBrush;
            set => SetProperty(ref _snackbarBackgroundBrush, value);
        }
        private MessageToSnackLevel _lastMessageLevel = MessageToSnackLevel.NoLevel;
        public MessageToSnackLevel LastMessageLevel
        {
            get => _lastMessageLevel;
            set => SetProperty(ref _lastMessageLevel, value);
        }


        public LocalMessagesViewModel LocalMessagesVM { get; }
        public XMLEditorViewModel XMLEditorVM { get; }
        public InitViewModel InitVM { get; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private readonly ObservableDiagnosticLogger _logger;

        public ICollectionView FilteredMessages { get; }

        private bool _showInfo = true;
        public bool ShowInfo
        {
            get => _showInfo;
            set
            {
                if (SetProperty(ref _showInfo, value))
                    FilteredMessages.Refresh();
            }
        }

        private bool _showWarning = true;
        public bool ShowWarning
        {
            get => _showWarning;
            set
            {
                if (SetProperty(ref _showWarning, value))
                    FilteredMessages.Refresh();
            }
        }

        private bool _showError = true;
        public bool ShowError
        {
            get => _showError;
            set
            {
                if (SetProperty(ref _showError, value))
                    FilteredMessages.Refresh();
            }
        }

        public ICommand ClearLogCommand { get; }
        public ICommand ShowInitCommand { get; }
        public ICommand ShowLocalMessagesCommand { get; }
        public ICommand ShowXMLEditorCommand { get; }

        public MainViewModel(LocalMessagesViewModel localMessagesVM, XMLEditorViewModel xmlEditorVM, InitViewModel initVM, ISnackBarManager snackBarManager, ObservableDiagnosticLogger logger)
        {
            LocalMessagesVM = localMessagesVM;
            XMLEditorVM = xmlEditorVM;
            InitVM = initVM;
            _logger = logger;
           
            CurrentView = InitVM;

            ShowLocalMessagesCommand = new RelayCommand(_ => CurrentView = LocalMessagesVM);
            ShowXMLEditorCommand = new RelayCommand(_ => CurrentView = XMLEditorVM);
            ShowInitCommand = new RelayCommand(_ => CurrentView = InitVM);
            ClearLogCommand = new RelayCommand(_ => _logger.Clear());

            FilteredMessages = CollectionViewSource.GetDefaultView(_logger.Messages);
            FilteredMessages.Filter = FilterMessages;

            _snackBarManager = snackBarManager;
            _snackBarManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_snackBarManager.CurrentMessage))
                {
                    if (_snackBarManager.CurrentMessage != null)
                    {
                        LastMessageLevel = _snackBarManager.CurrentMessage.Level;
                    }

                    OnPropertyChanged(nameof(CurrentMessage));
                    UpdateSnackbarBackgroundBrush();
                }
            };
        }

        private void UpdateSnackbarBackgroundBrush()
        {
            SnackbarBackgroundBrush = LastMessageLevel switch
            {
                MessageToSnackLevel.Error => (Brush)System.Windows.Application.Current.Resources["ColorSBError"],
                MessageToSnackLevel.Warning => (Brush)System.Windows.Application.Current.Resources["ColorSBWarning"],
                MessageToSnackLevel.Info => (Brush)System.Windows.Application.Current.Resources["ColorSBInfo"],
                MessageToSnackLevel.Success => (Brush)System.Windows.Application.Current.Resources["ColorSBSuccess"],
                _ => (Brush)System.Windows.Application.Current.Resources["ColorSBInfo"]
            };
        }

        private bool FilterMessages(object obj)
        {
            if (obj is not DiagnosticMessage msg)
                return false;

            return (msg.Level == DiagnosticLevel.Info && ShowInfo)
                || (msg.Level == DiagnosticLevel.Warning && ShowWarning)
                || (msg.Level == DiagnosticLevel.Error && ShowError);
        }
    }
}
