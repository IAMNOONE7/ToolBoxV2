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
        LocalMessageUpdate ApplyRow(ExcelRow row);
        IReadOnlyCollection<LocalMessage> GetAll();
    }
}
