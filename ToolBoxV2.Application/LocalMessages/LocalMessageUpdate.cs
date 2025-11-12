using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Domain.LocalMessages;

namespace ToolBoxV2.Application.LocalMessages
{
   
    // Describes what changed when a row was applied to the builder.    
    public enum LocalMessageUpdateKind
    {
        None,
        NewMessage,
        NewItem
    }

    // Small Application DTO emitted by the builder so callers (e.g., ViewModels) can
    // update UI efficiently (add a node, append a child, etc.) without re-scanning all data.

    public class LocalMessageUpdate
    {
        //Kind of update that occurred.
        public LocalMessageUpdateKind Kind { get; init; }
        public LocalMessage? Message { get; init; }          // set when new message
        public LocalMessageItem? Item { get; init; }         // set when new item
    }
}
