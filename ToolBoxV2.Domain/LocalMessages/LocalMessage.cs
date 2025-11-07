using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Domain.LocalMessages
{
    public class LocalMessage
    {
        public string Name { get; set; } = string.Empty;
        public List<LocalMessageItem> Items { get; set; } = new();
    }

    public class LocalMessageItem
    {
        public int Index { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
