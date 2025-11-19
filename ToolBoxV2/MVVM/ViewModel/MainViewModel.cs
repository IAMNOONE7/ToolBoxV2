using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxV2.Presentation.WPF.Core;
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

        public ICommand ShowInitCommand { get; }
        public ICommand ShowLocalMessagesCommand { get; }
        public ICommand ShowXMLEditorCommand { get; }

        public MainViewModel(LocalMessagesViewModel localMessagesVM, XMLEditorViewModel xmlEditorVM, InitViewModel initVM, ISnackBarManager snackBarManager)
        {
            LocalMessagesVM = localMessagesVM;
            XMLEditorVM = xmlEditorVM;
            InitVM = initVM;

            // initial view
            CurrentView = InitVM;

            ShowLocalMessagesCommand = new RelayCommand(_ => CurrentView = LocalMessagesVM);
            ShowXMLEditorCommand = new RelayCommand(_ => CurrentView = XMLEditorVM);
            ShowInitCommand = new RelayCommand(_ => CurrentView = InitVM);

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
    }
}
