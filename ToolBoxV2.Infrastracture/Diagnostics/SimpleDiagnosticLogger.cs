using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;

namespace ToolBoxV2.Infrastracture.Diagnostics
{
    public class SimpleDiagnosticLogger : IDiagnosticLogger
    {
        public void Info(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void Warn(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void Error(string message, Exception? ex = null)
        {
            Console.WriteLine($"[ERROR] {message}");
            if (ex != null)
                Console.WriteLine(ex);
        }
    }
}
