using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Application.Common
{
    public sealed class ExcelReadRequest
    {
        public string FilePath { get; init; } = string.Empty;
        public string SheetName { get; init; } = string.Empty;
        public int HeaderRowIndex { get; init; } = 1;
        public IReadOnlyList<string> ExpectedColumns { get; init; } = [];
    }

    public sealed class ExcelRow
    {
        // key = column name from header, value = cell value
        public Dictionary<string, object?> Cells { get; } = new();
    }

    public interface IExcelReader
    {
        
        // Reads Excel row-by-row from given sheet, mapping cells to expected column names.        
        IAsyncEnumerable<ExcelRow> ReadAsync(ExcelReadRequest request, CancellationToken cancellationToken = default);
    }
}
