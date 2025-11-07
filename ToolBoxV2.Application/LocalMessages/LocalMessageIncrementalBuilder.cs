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
        private readonly Dictionary<string, LocalMessage> _messages =
           new(StringComparer.OrdinalIgnoreCase);

        public LocalMessageUpdate ApplyRow(ExcelRow row)
        {
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
                lm = new LocalMessage
                {
                    Name = name,
                    Items = new List<LocalMessageItem>()
                };
                _messages.Add(name, lm);
                isNewMessage = true;
            }

            var item = new LocalMessageItem
            {
                Index = index,
                Text = text
            };
            lm.Items.Add(item);

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

        public IReadOnlyCollection<LocalMessage> GetAll() => _messages.Values;
    }
}
