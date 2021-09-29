using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPF.Core.Utilities
{
    public static class Logging
    {
        /// <summary>
        /// Process a chunk of text and send it to a handler
        /// </summary>
        /// <param name="reader">TextReader representing the input</param>
        /// <param name="baseClass">Invoking class, passed on to the event handler</param>
        /// <param name="handler">Event handler to be invoked to write to log</param>
        public static async Task OutputToLog(TextReader reader, object baseClass, EventHandler<string> handler)
        {
            // Initialize the required variables
            char[] buffer = new char[256];
            int read = 0;
            var sb = new StringBuilder();

            try
            {
                while (true)
                {
                    // Try to read the next chunk of characters
                    read = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    // Convert the characters into a string
                    string line = new string(buffer, 0, read);

                    // If we have no newline characters, store in the string builder
                    if (!line.Contains("\r") && !line.Contains("\n"))
                        sb.Append(line);

                    // If we have a newline, append and log
                    else if (line.Contains("\n") || line.Contains("\r\n"))
                        ProcessNewLines(sb, line, baseClass, handler);

                    // If we have a carriage return only, append and log first and last instances
                    else if (line.Contains("\r"))
                        ProcessCarriageReturns(sb, line, baseClass, handler);
                }
            }
            catch { }
            finally
            {
                handler?.Invoke(baseClass, sb.ToString());
            }
        }

        /// <summary>
        /// Process a chunk that contains newlines
        /// </summary>
        /// <param name="sb">StringBuilder to write from and append to</param>
        /// <param name="line">Current line to process</param>
        /// <param name="baseClass">Invoking class, passed on to the event handler</param>
        /// <param name="handler">Event handler to be invoked to write to log</param>
        private static void ProcessNewLines(StringBuilder sb, string line, object baseClass, EventHandler<string> handler)
        {
            line = line.Replace("\r\n", "\n");
            var split = line.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                // If the chunk contains a carriage return, handle it like a separate line
                if (split[i].Contains("\r"))
                {
                    ProcessCarriageReturns(sb, split[i], baseClass, handler);
                    continue;
                }

                // For the first item, append to anything existing and then write out
                if (i == 0)
                {
                    sb.Append(split[i]);
                    handler?.Invoke(baseClass, sb.ToString());
                    sb.Clear();
                }

                // For the last item, just append so it's dealt with the next time
                else if (i == split.Length - 1)
                {
                    sb.Append(split[i]);
                }

                // For everything else, directly write out
                else
                {
                    handler?.Invoke(baseClass, split[i]);
                }
            }
        }

        /// <summary>
        /// Process a chunk that contains carriage returns
        /// </summary>
        /// <param name="sb">StringBuilder to write from and append to</param>
        /// <param name="line">Current line to process</param>
        /// <param name="baseClass">Invoking class, passed on to the event handler</param>
        /// <param name="handler">Event handler to be invoked to write to log</param>
        private static void ProcessCarriageReturns(StringBuilder sb, string line, object baseClass, EventHandler<string> handler)
        {
            var split = line.Split('\r');

            // Append and log the first
            sb.Append(split[0]);
            handler?.Invoke(baseClass, sb.ToString());

            // Append the last
            sb.Clear();
            sb.Append($"\r{split[split.Length - 1]}");
        }
    }
}
