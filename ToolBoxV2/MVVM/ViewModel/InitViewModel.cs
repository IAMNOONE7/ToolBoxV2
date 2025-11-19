using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Presentation.WPF.Core;
using ToolBoxV2.Presentation.WPF.Services.Diagnostics;

namespace ToolBoxV2.Presentation.WPF.MVVM.ViewModel
{    
    public class InitViewModel
    {
        public ICommand OpenManualCommand { get; }
        private readonly IDiagnosticLogger _logger;
        public InitViewModel(IDiagnosticLogger logger)
        {
            _logger = logger;
            OpenManualCommand = new RelayCommand(_ => OpenManual());
        }

        public void OpenManual()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var pdfPath = Path.Combine(baseDir, "Docs", "Manual.pdf");
                if (File.Exists(pdfPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pdfPath,
                        UseShellExecute = true
                    });

                    _logger.Info("Manual opened successfully.");
                }
                else
                {
                    _logger.Warn("Manual file not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to open manual: {ex.Message}");
            }
        }
    }
}
