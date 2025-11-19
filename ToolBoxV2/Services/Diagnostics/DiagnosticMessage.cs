using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Presentation.WPF.Services.Diagnostics
{
    public enum DiagnosticLevel
    {
        Info,
        Warning,
        Error
    }

    public class DiagnosticMessage
    {
        public DateTime Timestamp { get; set; }
        public DiagnosticLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ExceptionText { get; set; }

        public override string ToString()
            => $"{Timestamp:HH:mm:ss} [{Level}] {Message}";
    }
}
