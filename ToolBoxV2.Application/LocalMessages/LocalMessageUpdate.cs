using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Domain.LocalMessages;

namespace ToolBoxV2.Application.LocalMessages
{
    public enum LocalMessageUpdateKind
    {
        None,
        NewMessage,
        NewItem
    }

    public class LocalMessageUpdate
    {
        public LocalMessageUpdateKind Kind { get; init; }
        public LocalMessage? Message { get; init; }          // set when new message
        public LocalMessageItem? Item { get; init; }         // set when new item
    }
}
