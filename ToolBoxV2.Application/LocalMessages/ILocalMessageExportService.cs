using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Domain.LocalMessages;

namespace ToolBoxV2.Application.LocalMessages
{
    public interface ILocalMessageExportService
    {
        Task<LocalMessageExportResult> ExportAsync(
            string targetFolder,
            IEnumerable<LocalMessage> messages,
            CancellationToken cancellationToken = default);
    }

    public sealed class LocalMessageExportResult
    {
        public int SuccessCount { get; init; }
        public int FailureCount { get; init; }
        public bool HasErrors => FailureCount > 0;
    }
}
