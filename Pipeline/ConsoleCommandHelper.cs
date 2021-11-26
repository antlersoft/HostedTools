using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Pipeline
{
      public static class ConsoleCommandHelper
    {
        public static Task<int> InvokeConsoleCommand(this IWorkMonitor monitor, ProcessStartInfo startInfo, List<Process> startedProcesses = null, Stream input = null)
        {
            return InvokeConsoleCommand(monitor, startInfo, CancellationToken.None, startedProcesses, input);
        }

        public static async Task<int> InvokeConsoleCommand(this IWorkMonitor monitor, ProcessStartInfo startInfo,  CancellationToken token, List<Process> startedProcesses = null, Stream input = null)
        {
            Process process;
            var tasks = new List<Task>();
            if (startInfo.UseShellExecute)
            {
                // Just run it with nothing fancy
                process = Process.Start(startInfo);
                if (startedProcesses != null)
                {
                    startedProcesses.Add(process);
                }
            }
            else
            {
                if (input != null)
                {
                    startInfo.RedirectStandardInput = true;
                }
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                process = Process.Start(startInfo);
                if (startedProcesses != null)
                {
                    startedProcesses.Add(process);
                }
                if (input != null)
                {
                    tasks.Add(CopyInput(process, input, token));
                }
                tasks.Add(CopyOutput(process.StandardError, monitor.Writer, token));
                tasks.Add(CopyOutput(process.StandardOutput, monitor.Writer, token));
            }
            int result = -1;
            await Task.WhenAll(tasks).ConfigureAwait(false);
            while (! process.HasExited && !token.IsCancellationRequested)
            {
                await Task.Delay(100, token).ConfigureAwait(false);
            }
            return result;
        }

        private const int BUFFER_SIZE = 1024;

        private static async Task CopyInput(Process process, Stream input, CancellationToken token)
        {
            byte[] buffer = new byte[BUFFER_SIZE];
            var encoding = Encoding.UTF8;
            while (! token.IsCancellationRequested)
            {
                int count = await input.ReadAsync(buffer, 0, BUFFER_SIZE).ConfigureAwait(false);
                if (count > 0)
                {
                    await process.StandardInput.WriteAsync(encoding.GetChars(buffer, 0, count)).ConfigureAwait(false);
                }
                else
                {
                    process.StandardInput.Close();
                    break;
                }
            }
        }

        private static async Task CopyOutput(StreamReader fromProcess, TextWriter toMonitor, CancellationToken token)
        {
            for (string line; ! token.IsCancellationRequested && (line = await fromProcess.ReadLineAsync().ConfigureAwait(false)) != null;)
            {
                await toMonitor.WriteLineAsync(line).ConfigureAwait(false);
            }
        }
    }

}