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
#if NET40
        public static async Task OutputToLog(TextReader reader, object baseClass, EventHandler<Modules.BaseParameters.StringEventArgs>? handler)
#else
        public static async Task OutputToLog(TextReader reader, object baseClass, EventHandler<string>? handler)
#endif
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
#if NET40
                    read = await Task.Factory.StartNew(() => reader.Read(buffer, 0, buffer.Length));
#else
                    read = await reader.ReadAsync(buffer, 0, buffer.Length);
#endif
                    if (read == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    // Convert the characters into a string
                    string line = new(buffer, 0, read);

                    // If we have no newline characters, store in the string builder
#if NETFRAMEWORK
                    if (!line.Contains("\r") && !line.Contains("\n"))
#else
                    if (!line.Contains('\r') && !line.Contains('\n'))
#endif
                        sb.Append(line);

                    // If we have a newline, append and log
#if NETFRAMEWORK
                    else if (line.Contains("\n") || line.Contains("\r\n"))
#else
                    else if (line.Contains('\n') || line.Contains("\r\n"))
#endif
                        ProcessNewLines(sb, line, baseClass, handler);

                    // If we have a carriage return only, append and log first and last instances
#if NETFRAMEWORK
                    else if (line.Contains("\r"))
#else
                    else if (line.Contains('\r'))
#endif
                        ProcessCarriageReturns(sb, line, baseClass, handler);
                }
            }
            catch { }
            finally
            {
#if NET40
                handler?.Invoke(baseClass, new Modules.BaseParameters.StringEventArgs { Value = sb.ToString() });
#else
                handler?.Invoke(baseClass, sb.ToString());
#endif
            }
        }

        /// <summary>
        /// Process a chunk that contains newlines
        /// </summary>
        /// <param name="sb">StringBuilder to write from and append to</param>
        /// <param name="line">Current line to process</param>
        /// <param name="baseClass">Invoking class, passed on to the event handler</param>
        /// <param name="handler">Event handler to be invoked to write to log</param>
#if NET40
        private static void ProcessNewLines(StringBuilder sb, string line, object baseClass, EventHandler<Modules.BaseParameters.StringEventArgs>? handler)
#else
        private static void ProcessNewLines(StringBuilder sb, string line, object baseClass, EventHandler<string>? handler)
#endif
        {
            line = line.Replace("\r\n", "\n");
            var split = line.Split('\n');
            for (int i = 0; i < split.Length; i++)
            {
                // If the chunk contains a carriage return, handle it like a separate line
#if NETFRAMEWORK
                if (split[i].Contains("\r"))
#else
                if (split[i].Contains('\r'))
#endif
                {
                    ProcessCarriageReturns(sb, split[i], baseClass, handler);
                    continue;
                }

                // For the first item, append to anything existing and then write out
                if (i == 0)
                {
                    sb.Append(split[i]);
#if NET40
                    handler?.Invoke(baseClass, new Modules.BaseParameters.StringEventArgs { Value = sb.ToString() });
#else
                    handler?.Invoke(baseClass, sb.ToString());
#endif
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
#if NET40
                    handler?.Invoke(baseClass, new Modules.BaseParameters.StringEventArgs { Value = split[i] });
#else
                    handler?.Invoke(baseClass, split[i]);
#endif
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
#if NET40
        private static void ProcessCarriageReturns(StringBuilder sb, string line, object baseClass, EventHandler<Modules.BaseParameters.StringEventArgs>? handler)
#else
        private static void ProcessCarriageReturns(StringBuilder sb, string line, object baseClass, EventHandler<string>? handler)
#endif
        {
            var split = line.Split('\r');

            // Append and log the first
            sb.Append(split[0]);
#if NET40
            handler?.Invoke(baseClass, new Modules.BaseParameters.StringEventArgs { Value = sb.ToString() });
#else
            handler?.Invoke(baseClass, sb.ToString());
#endif

            // Append the last
            sb.Clear();
            sb.Append($"\r{split[split.Length - 1]}");
        }
    }
}
