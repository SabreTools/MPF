using System;
using System.Collections.Generic;
using System.Text;

namespace MPF.Avalonia.Helpers
{
    /// <summary>
    /// Minimal terminal line-discipline for redirected child-process output.
    ///
    /// The point: read RAW characters from stdout (never <c>ReadLine</c>) and
    /// interpret carriage returns the way a real terminal does. Dump tools
    /// (redumper, DiscImageCreator, Aaru) update their progress with a bare
    /// <c>\r</c> (no <c>\n</c>) so the same physical line is redrawn in place.
    /// <c>ReadLine</c>/<c>OutputDataReceived</c> treat <c>\r</c> as a line break,
    /// which explodes one updating line into thousands of separate lines -- the
    /// spam + lag the maintainer saw when output was redirected years ago.
    ///
    /// Here, <c>\r</c> moves the write cursor to column 0 of the live line
    /// (overwrite, not new line); <c>\n</c> commits the live line. Progress
    /// updates therefore mutate a single live line and never grow the log.
    ///
    /// No UI dependency: pure and unit-testable. The UI binds the committed lines
    /// to a virtualized list and the live line to one trailing row.
    /// </summary>
    public sealed class TerminalBuffer
    {
        private readonly StringBuilder _current = new();

        /// <summary>
        /// Write column within <see cref="_current"/>
        /// </summary>
        private int _cursor;

        /// <summary>
        /// Mid ANSI escape sequence (survives chunk boundaries)
        /// </summary>
        private bool _inEscape;

        private readonly StringBuilder _escape = new();

        /// <summary>
        /// Lines committed so far (each was terminated by <c>\n</c>).
        /// </summary>
        public List<string> CommittedLines { get; } = [];

        /// <summary>
        /// The live line being assembled after the last <c>\n</c>.
        /// </summary>
        public string CurrentLine => _current.ToString();

        /// <summary>
        /// Raised once per committed line.
        /// </summary>
        public event Action<string>? LineCommitted;

        /// <summary>
        /// Raised when the live line changes (printable char, <c>\r</c> overwrite, erase).
        /// </summary>
        public event Action<string>? CurrentLineChanged;

        /// <summary>
        /// Feed a raw chunk of output. Chunk boundaries are arbitrary: a <c>\r\n</c>
        /// or an escape sequence may be split across two calls and is handled.
        /// </summary>
        public void Feed(string? chunk)
        {
            if (string.IsNullOrEmpty(chunk))
                return;

            bool liveChanged = false;
            for (int i = 0; i < chunk.Length; i++)
            {
                char c = chunk[i];

                if (_inEscape)
                {
                    liveChanged |= FeedEscape(c);
                    continue;
                }

                switch (c)
                {
                    // ESC: start of an ANSI sequence
                    case '\x1b':
                        _inEscape = true;
                        _escape.Clear();
                        break;

                    case '\n':
                        CommitLine();
                        break;

                    // Carriage Return: back to column 0, keep the chars
                    case '\r':
                        _cursor = 0;
                        break;

                    case '\t':
                        do { WriteChar(' '); } while (_cursor % 8 != 0);
                        liveChanged = true;
                        break;

                    case '\b':
                        if (_cursor > 0)
                            _cursor--;

                        break;

                    default:
                        if (!char.IsControl(c))
                        {
                            WriteChar(c);
                            liveChanged = true;
                        }
                        // other control chars (bell, etc.) are dropped
                        break;
                }
            }

            if (liveChanged)
                CurrentLineChanged?.Invoke(CurrentLine);
        }

        /// <summary>
        /// Force-commit whatever is in the live line (e.g. on process exit).
        /// </summary>
        public void Flush()
        {
            if (_current.Length > 0)
                CommitLine();
        }

        /// <summary>
        /// Write a single character to the current output
        /// </summary>
        private void WriteChar(char c)
        {
            if (_cursor < _current.Length)
                _current[_cursor] = c;
            else
                _current.Append(c);

            _cursor++;
        }

        /// <summary>
        /// Commit a complete line to the output
        /// </summary>
        private void CommitLine()
        {
            string line = _current.ToString();
            CommittedLines.Add(line);
            LineCommitted?.Invoke(line);

            _current.Clear();
            _cursor = 0;
        }

        /// <summary>
        /// Consume one character of an in-progress escape sequence. We only model
        /// the few CSI sequences progress writers actually use; everything else
        /// (colors, cursor moves) is recognized and stripped so it never shows as
        /// literal garbage. Returns whether the live line changed.
        /// </summary>
        private bool FeedEscape(char c)
        {
            // First char after ESC decides the family.
            if (_escape.Length == 0)
            {
                if (c == '[')
                {
                    _escape.Append(c); // CSI -- keep collecting until a final byte
                    return false;
                }

                // Not a CSI (e.g. ESC( charset, ESC] OSC). We don't model these;
                // drop ESC + this byte and resume. Good enough for tool output.
                _inEscape = false;
                return false;
            }

            // Inside CSI: parameters/intermediates until a final byte 0x40-0x7E.
            if (c >= '@' && c <= '~')
            {
                bool changed = ApplyCSISequence(_escape.ToString(1, _escape.Length - 1), c);
                _inEscape = false;
                _escape.Clear();
                return changed;
            }

            _escape.Append(c);
            return false;
        }

        /// <summary>
        /// Apply a CSI sequence given its parameter string and final byte.
        /// </summary>
        private bool ApplyCSISequence(string parameters, char final)
        {
            switch (final)
            {
                // EL -- erase in line
                case 'K':
                    EraseInLine(ParseIntWithFallback(parameters, 0));
                    return true;

                // CHA -- cursor to absolute column (1-based)
                case 'G':
                    _cursor = Math.Max(0, ParseIntWithFallback(parameters, 1) - 1);
                    return false;

                // CUB -- cursor back N
                case 'D':
                    _cursor = Math.Max(0, _cursor - Math.Max(1, ParseIntWithFallback(parameters, 1)));
                    return false;

                // SGR colors ('m'), cursor up/down, etc. -- recognized and ignored.
                default:
                    return false;
            }
        }

        /// <summary>
        /// Erase characters inside the current line
        /// </summary>
        /// <param name="mode">Mode determining how to erase characters</param>
        /// TODO: Determine if mode could be an enum to bound this better
        private void EraseInLine(int mode)
        {
            switch (mode)
            {
                // Cursor to end of line
                case 0:
                    if (_cursor < _current.Length)
                        _current.Length = _cursor;

                    break;

                // Start of line to cursor (blank them, keep length)
                case 1:
                    for (int i = 0; i < _cursor && i < _current.Length; i++)
                    {
                        _current[i] = ' ';
                    }

                    break;

                // Entire line
                case 2:
                    _current.Clear();
                    _cursor = 0;
                    break;

                // Ignore all other mode values
                default:
                    break;
            }
        }

        /// <summary>
        /// Try to parse an integer value, including a fallback if it fails
        /// </summary>
        private static int ParseIntWithFallback(string s, int fallback)
            => int.TryParse(s, out int v) ? v : fallback;
    }
}
