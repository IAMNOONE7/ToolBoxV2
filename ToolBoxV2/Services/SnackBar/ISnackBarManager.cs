using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Presentation.WPF.Services.SnackBar
{
    public interface ISnackBarManager : INotifyPropertyChanged
    {
        MessageToSnack CurrentMessage { get; }

        Task EnqueueMessageAsync(MessageToSnack snackMsg);
    }
}
