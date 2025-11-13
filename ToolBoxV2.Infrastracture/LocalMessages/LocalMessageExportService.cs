using DocumentFormat.OpenXml.Spreadsheet;
using OpenMcdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Application.LocalMessages;
using ToolBoxV2.Domain.LocalMessages;

namespace ToolBoxV2.Infrastracture.LocalMessages
{
    public class LocalMessageExportService : ILocalMessageExportService
    {
        private readonly IDiagnosticLogger _logger;
        public LocalMessageExportService(IDiagnosticLogger logger)
        {
            _logger = logger;
        }

        public async Task<LocalMessageExportResult> ExportAsync(
          string targetFolder,
          IEnumerable<LocalMessage> messages,
          CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentException("Target folder cannot be empty.", nameof(targetFolder));

            if (!Directory.Exists(targetFolder))
                throw new DirectoryNotFoundException($"Target folder '{targetFolder}' does not exist.");

            if (messages is null)
                throw new ArgumentNullException(nameof(messages));

            var messageList = messages.ToList();
            if (messageList.Count == 0)
                return new LocalMessageExportResult { SuccessCount = 0, FailureCount = 0 };

            int success = 0;
            int failures = 0;

            await Task.Run(() =>
            {
                foreach (var message in messageList)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var items = message.Items?.ToList() ?? new List<LocalMessageItem>();

                        // 1) Validate unique indices
                        var duplicateIndices = items
                            .GroupBy(i => i.Index)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (duplicateIndices.Count > 0)
                        {
                            failures++;
                            _logger.Error(
                                $"Local Message '{message.Name}' has duplicate indices: {string.Join(", ", duplicateIndices)}. " +
                                "Skipping .loc generation.");
                            continue; // do NOT call WriteLocFile
                        }
                        var issue = WriteLocFile(targetFolder, message);
                        if (issue)
                        {
                            failures++;
                        }
                        else
                        {
                            success++;
                            _logger.Info($"Generated: {message.Name}.loc");
                        }
                    }
                    catch (Exception ex)
                    {
                        failures++;
                        _logger.Error($"Error writing {message.Name}.loc", ex);
                    }
                }
            }, cancellationToken);

            return new LocalMessageExportResult
            {
                SuccessCount = success,
                FailureCount = failures
            };
        }

        private bool WriteLocFile(string targetFolder, LocalMessage message)
        {
            var items = message.Items?.ToList() ?? new List<LocalMessageItem>();
            bool issue = false;

            CompoundFile cf = new CompoundFile();
            try
            {
                var ls409 = cf.RootStorage.AddStream("ls00000409");
                var messagesStream = cf.RootStorage.AddStream("Messages");

                messagesStream.SetData(BuildMessagesStream(items));
                ls409.SetData(BuildLs409Stream(items));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating .loc file structure in {message.Name}.", ex);
                issue = true;
            }

            try
            {
                var fileName = Path.Combine(targetFolder, $"{message.Name}.loc");
                cf.SaveAs(fileName);
                cf.Close();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving .loc file {message.Name} to disk.", ex);
                issue = true;
            }

            return issue;
        }

        private static byte[] BuildMessagesStream(IReadOnlyList<LocalMessageItem> items)
        {
            List<byte> data = new List<byte>();
            data.Add(5);  // Format version
            data.Add(0);  // Reserved
            data.AddRange(BitConverter.GetBytes((ushort)items.Count)); // Total triggers (2 bytes)
            data.Add(3);  // Version control
            data.AddRange(new byte[3]); // Reserved padding
            int index = 1;
            foreach (var kvp in items)
            {
                data.AddRange(BitConverter.GetBytes(kvp.Index)); // Trigger value (4 bytes)
                data.AddRange(BitConverter.GetBytes(index));    // Entry number (4 bytes)
                index++;
            }
            return data.ToArray();
        }

        private static byte[] BuildLs409Stream(IReadOnlyList<LocalMessageItem> items)
        {
            List<byte> data = new List<byte>();
            List<byte> messageData = new List<byte>();
            List<long> pointers = new List<long>();

            // Header: Format version and total messages
            data.Add(1); // Format version
            data.Add(0); // Reserved
            data.AddRange(BitConverter.GetBytes((ushort)items.Count)); // Total messages (2 bytes)
            data.AddRange(new byte[6]);
            long currentOffset = items.Count * 12 + 10;
            foreach (var kvp in items)
            {
                pointers.Add(currentOffset);
                data.AddRange(BitConverter.GetBytes(pointers.Count));
                data.AddRange(BitConverter.GetBytes((currentOffset - pointers[0]) / 2));

                byte[] unicodeBytes = Encoding.Unicode.GetBytes(kvp.Text);
                messageData.AddRange(unicodeBytes);

                currentOffset += unicodeBytes.Length;
            }
            data.AddRange(messageData);
            return data.ToArray();
        }
    }
}
