using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Application.Common
{
    /// <summary>
    /// Describes a request to read structured data from an Excel sheet.
    ///
    /// <para>Layer & role:</para>
    /// <b>Application layer</b> — this is a lightweight data contract used by
    /// <see cref="IExcelReader"/> to specify what file and sheet to read.
    /// 
    /// It is intentionally free of any specific library references (e.g., ClosedXML),
    /// so the Infrastructure layer can implement the actual Excel reading logic
    /// using any backend library.
    /// </summary>
    
    public sealed class ExcelReadRequest
    {
        public string FilePath { get; init; } = string.Empty;
        public string SheetName { get; init; } = string.Empty;
        public int HeaderRowIndex { get; init; } = 1;

        // Optional list of expected column headers.
        // Implementations may validate that these columns exist
        // or use them to guide column matching logic.
        public IReadOnlyList<string>? ExpectedColumns { get; init; }
    }

    public sealed class ExcelRow
    {
        // Represents a single row of Excel data after being mapped
        // key = column name from header, value = cell value
        public Dictionary<string, object?> Cells { get; } = new();
    }

    // Application interface — defines how higher layers (like ViewModels)
    // can request Excel data without depending on a specific file format library.

    // The actual implementation (e.g., ClosedXMLExcelReader) lives in the
    // Infrastructure layer and converts the raw Excel file into a sequence of
    /// <see cref="ExcelRow"/> objects.

    public interface IExcelReader
    {
        
        // Reads Excel row-by-row from given sheet, mapping cells to expected column names.        
        IAsyncEnumerable<ExcelRow> ReadAsync(ExcelReadRequest request, CancellationToken cancellationToken = default);
    }
}
