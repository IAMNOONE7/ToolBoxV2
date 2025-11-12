using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Domain.LocalMessages;

namespace ToolBoxV2.Application.LocalMessages
{
    public class LocalMessageIncrementalBuilder : ILocalMessageIncrementalBuilder
    {
        /// <summary>
        /// Incrementally builds a set of <see cref="LocalMessage"/> objects from Excel rows.
        /// 
        /// Layer & role:
        /// Application (use-case). Pure business logic that coordinates Domain types.
        /// No IO or UI; consumes simple inputs (Excel rows already read elsewhere) and
        /// returns structured Domain entities and small status DTOs.
        /// 
        /// Lifecycle:
        /// Treat as short-lived. Create a fresh instance for each import run to avoid
        /// shared state. Prefer resolving via <see cref="ILocalMessageIncrementalBuilderFactory"/>.
        /// </summary>

        // Internal store keyed by message name (case-insensitive).
        private readonly Dictionary<string, LocalMessage> _messages =
           new(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Applies a single Excel row to the current build, returning a small update
        /// that tells the caller whether a new message was created or an item was appended.
        /// </summary>
        /// <param name="row">A parsed Excel row (Application DTO) with cells by header.</param>
        /// <returns>
        /// <see cref="LocalMessageUpdate"/> with Kind = None/NewMessage/NewItem.
        /// </returns>
        public LocalMessageUpdate ApplyRow(ExcelRow row)
        {
            // Extract fields defensively from row cells (headers are normalized strings).
            var name = row.Cells.TryGetValue("Name", out var n) ? n?.ToString() ?? "" : "";
            var text = row.Cells.TryGetValue("Text", out var t) ? t?.ToString() ?? "" : "";
            var index = row.Cells.TryGetValue("Index", out var i) && int.TryParse(i?.ToString(), out var v) ? v : 0;

            if (string.IsNullOrWhiteSpace(name))
            {
                return new LocalMessageUpdate { Kind = LocalMessageUpdateKind.None };
            }

            var isNewMessage = false;
            if (!_messages.TryGetValue(name, out var lm))
            {
                // New message shell (Domain entity) is created and tracked.
                lm = new LocalMessage
                {
                    Name = name,
                    Items = new List<LocalMessageItem>()
                };
                _messages.Add(name, lm);
                isNewMessage = true;
            }

            // Append a new item to the message.
            var item = new LocalMessageItem
            {
                Index = index,
                Text = text
            };
            lm.Items.Add(item);

            // Append a new item to the message.
            if (isNewMessage)
            {
                return new LocalMessageUpdate
                {
                    Kind = LocalMessageUpdateKind.NewMessage,
                    Message = lm,
                    Item = item
                };
            }

            return new LocalMessageUpdate
            {
                Kind = LocalMessageUpdateKind.NewItem,
                Message = lm,
                Item = item
            };
        }
        // Returns all built messages so far. Call this after processing all rows.
        public IReadOnlyCollection<LocalMessage> GetAll() => _messages.Values;
    }
}
