using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxV2.Presentation.WPF.Core;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
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

        public MainViewModel(LocalMessagesViewModel localMessagesVM, XMLEditorViewModel xmlEditorVM, InitViewModel initVM)
        {
            LocalMessagesVM = localMessagesVM;
            XMLEditorVM = xmlEditorVM;
            InitVM = initVM;

            // initial view
            CurrentView = InitVM;

            ShowLocalMessagesCommand = new RelayCommand(_ => CurrentView = LocalMessagesVM);
            ShowXMLEditorCommand = new RelayCommand(_ => CurrentView = XMLEditorVM);
            ShowInitCommand = new RelayCommand(_ => CurrentView = InitVM);
        }
    }
}
