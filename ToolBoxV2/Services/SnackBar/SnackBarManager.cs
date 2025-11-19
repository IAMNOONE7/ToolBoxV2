using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Presentation.WPF.Core;

namespace ToolBoxV2.Presentation.WPF.Services.SnackBar
{
    public class SnackBarManager : ObservableObject, ISnackBarManager
    {
        private readonly Queue<MessageToSnack> _messageQueue = new();
        private bool _isProcessing = false;

        private MessageToSnack _currentMessage;
        public MessageToSnack CurrentMessage
        {
            get => _currentMessage;
            private set => SetProperty(ref _currentMessage, value);
        }

        public async Task EnqueueMessageAsync(MessageToSnack message)
        {
            _messageQueue.Enqueue(message);

            // Start processing only if not already doing so
            if (!_isProcessing)
                await ProcessQueueAsync();
        }

        private async Task ProcessQueueAsync()
        {
            _isProcessing = true;

            while (_messageQueue.Count > 0)
            {
                var msg = _messageQueue.Dequeue();
                CurrentMessage = msg;

                // Wait for the layout/render cycle to catch up
                await Task.Yield(); // Let WPF render CurrentMessage

                // Wait for actual UI time, not just async delay
                await WaitWithDispatcherAsync(msg.Duration ?? TimeSpan.FromSeconds(3));

                await Task.Delay(400);

                CurrentMessage = null;

                // Short pause between messages
                await Task.Delay(300);
            }

            _isProcessing = false;
        }

        private Task WaitWithDispatcherAsync(TimeSpan duration)
        {
            var tcs = new TaskCompletionSource<bool>();
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = duration
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                tcs.SetResult(true);
            };
            timer.Start();
            return tcs.Task;
        }
    }
}
