using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Domain.LocalMessages;

namespace ToolBoxV2.Application.LocalMessages
{ 
    public interface ILocalMessageIncrementalBuilder
    {
        
        // Processes a single row of Excel data and applies it to the current in-memory build.
        // Returns a lightweight update object describing what changed.            
        // returns
        /// A <see cref="LocalMessageUpdate"/> indicating whether a new message or item
        /// was created.       
        LocalMessageUpdate ApplyRow(ExcelRow row);

        
        /// Returns all accumulated <see cref="LocalMessage"/> objects after one or more
        /// rows have been applied.        
        IReadOnlyCollection<LocalMessage> GetAll();
    }
}
