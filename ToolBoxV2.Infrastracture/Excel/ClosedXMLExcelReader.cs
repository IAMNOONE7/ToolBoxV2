using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;


namespace ToolBoxV2.Infrastracture.Excel
{
    /// <summary>
    /// Concrete implementation of <see cref="IExcelReader"/> that uses
    /// the ClosedXML library to read Excel files.
    ///
    /// Layer  role:
    /// Infrastructure layer — performs the actual IO and file parsing,
    /// while exposing the Application-defined abstraction (<see cref="IExcelReader"/>)
    /// so that upper layers (e.g., ViewModels) remain library-agnostic.
    ///
    /// Behavior:
    /// - Reads a specified worksheet row-by-row.  
    /// - Maps cell values to header names from the header row.  
    /// - Yields <see cref="ExcelRow"/> objects asynchronously, allowing
    ///   the UI to remain responsive during long imports.  
    /// - Automatically filters columns if <see cref="ExcelReadRequest.ExpectedColumns"/> is provided.
    /// </summary>
    
    public class ClosedXMLExcelReader : IExcelReader
    {
        /// <summary>
        /// Asynchronously reads rows from the specified Excel sheet and yields
        /// <see cref="ExcelRow"/> objects containing key-value pairs (column name → cell value).
        /// </summary>
        /// Params:
        /// request - Configuration describing which file, sheet, and header row to read.
        /// cancellationToke - Optional cancellation token for responsive cancellation
        /// returns
        /// An async stream of <see cref="ExcelRow"/> values
        public async IAsyncEnumerable<ExcelRow> ReadAsync(
          ExcelReadRequest request,
          [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                yield break;
            if (string.IsNullOrWhiteSpace(request.SheetName))
                yield break;

            using var workbook = new XLWorkbook(request.FilePath);
            var ws = workbook.Worksheet(request.SheetName);
            if (ws == null)
                yield break;

            // 1) read header row
            var headerRow = ws.Row(request.HeaderRowIndex);
            // buildColumnMap will return:
            // - only requested columns, if ExpectedColumns not empty
            // - ALL header columns, if ExpectedColumns empty
            var columnMap = BuildColumnMap(headerRow, request.ExpectedColumns);

            // 2) start reading from next row
            var currentRow = request.HeaderRowIndex + 1;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = ws.Row(currentRow);
                if (row.IsEmpty())
                    break;

                var excelRow = new ExcelRow();

                // we must iterate over the *map* (the actual columns we decided to use),
                // not over request.ExpectedColumns, because that might be empty.
                foreach (var kvp in columnMap)
                {
                    var colName = kvp.Key;
                    var colNumber = kvp.Value;

                    var cellVal = row.Cell(colNumber).Value;
                    excelRow.Cells[colName] = cellVal;
                }

                yield return excelRow;

                currentRow++;

                // let UI breathe
                await Task.Yield();
            }
        }
        
        // Builds a mapping between column headers (names) and their column numbers
        // in the Excel sheet.  
        // - If expectedColumns is empty, all non-empty headers are included.  
        // - If not empty, only columns explicitly listed are mapped.
         
        private Dictionary<string, int> BuildColumnMap(IXLRow headerRow, IReadOnlyList<string> expectedColumns)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // First, read all header cells into a lookup: headerText -> columnIndex
            var headerLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.Cells())
            {
                var text = cell.GetString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // ColumnNumber() is 1-based
                    headerLookup[text] = cell.Address.ColumnNumber;
                }
            }

            if (expectedColumns.Count == 0)
            {

                map = headerLookup
                    .Where(x => !string.IsNullOrEmpty(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

                return map;
            }

            // Now match only expected columns
            foreach (var expected in expectedColumns)
            {
                if (headerLookup.TryGetValue(expected, out var colNumber))
                {
                    map[expected] = colNumber;
                }
                // else: leave it out, reader will return null for this column
            }

            return map;
        }
    }
}
