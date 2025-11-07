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
    public class ClosedXMLExcelReader : IExcelReader
    {
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

                foreach (var expected in request.ExpectedColumns)
                {
                    if (columnMap.TryGetValue(expected, out var colNumber))
                    {
                        var cell = row.Cell(colNumber);
                        // ClosedXML gives you object-like value
                        var cellVal = cell.Value;
                        excelRow.Cells[expected] = cellVal;
                    }
                    else
                    {
                        // header didn't contain this expected column
                        excelRow.Cells[expected] = null;
                    }
                }

                yield return excelRow;

                currentRow++;

                // let UI breathe
                await Task.Yield();
            }
        }

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
