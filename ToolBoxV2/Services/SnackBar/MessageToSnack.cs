using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Presentation.WPF.Services.SnackBar
{
    public class MessageToSnack
    {
        public string Content { get; set; } = "";
        public MessageToSnackLevel Level { get; set; } = MessageToSnackLevel.NoLevel;
        public TimeSpan? Duration { get; set; } = null; // Default Material Design duration = 3s
        public bool WithCloseButton { get; set; } = true;
    }
}
