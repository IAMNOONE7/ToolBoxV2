using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;

namespace ToolBoxV2.Presentation.WPF.Services.Diagnostics
{
    public class ObservableDiagnosticLogger : IDiagnosticLogger
    {
        private readonly ObservableCollection<DiagnosticMessage> _messages = new();
        public ReadOnlyObservableCollection<DiagnosticMessage> Messages { get; }

        public ObservableDiagnosticLogger()
        {
            Messages = new ReadOnlyObservableCollection<DiagnosticMessage>(_messages);
        }

        private void Log(DiagnosticLevel level, string message, Exception? ex = null)
        {
            void Add()
            {
                _messages.Add(new DiagnosticMessage
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message,
                    ExceptionText = ex?.ToString()
                });
            }

            if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == true)
                Add();
            else
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke((Action)Add);
        }

        public void Info(string message) => Log(DiagnosticLevel.Info, message);
        public void Warn(string message) => Log(DiagnosticLevel.Warning, message);
        public void Error(string message, Exception? ex = null) => Log(DiagnosticLevel.Error, message, ex);

        public void Clear()
        {
            void DoClear() => _messages.Clear();

            if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == true)
                DoClear();
            else
                System.Windows.Application.Current?.Dispatcher?.BeginInvoke((Action)DoClear);
        }
    }
}
