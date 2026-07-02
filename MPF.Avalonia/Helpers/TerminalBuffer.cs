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
        public List<string> CommittedLines { get; } = new();

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
                    case '\x1b': // ESC -- start of an ANSI sequence
                        _inEscape = true;
                        _escape.Clear();
                        break;

                    case '\n':
                        CommitLine();
                        break;

                    case '\r':
                        _cursor = 0; // carriage return: back to column 0, keep the chars
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

        private void WriteChar(char c)
        {
            if (_cursor < _current.Length)
                _current[_cursor] = c;
            else
                _current.Append(c);

            _cursor++;
        }

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
                bool changed = ApplyCsi(_escape.ToString(1, _escape.Length - 1), c);
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
        private bool ApplyCsi(string parameters, char final)
        {
            switch (final)
            {
                case 'K': // EL -- erase in line
                    EraseInLine(ParseInt(parameters, 0));
                    return true;

                case 'G': // CHA -- cursor to absolute column (1-based)
                    _cursor = Math.Max(0, ParseInt(parameters, 1) - 1);
                    return false;

                case 'D': // CUB -- cursor back N
                    _cursor = Math.Max(0, _cursor - Math.Max(1, ParseInt(parameters, 1)));
                    return false;

                default:
                    // SGR colors ('m'), cursor up/down, etc. -- recognized and ignored.
                    return false;
            }
        }

        private void EraseInLine(int mode)
        {
            switch (mode)
            {
                case 0: // cursor to end of line
                    if (_cursor < _current.Length)
                        _current.Length = _cursor;

                    break;
                case 1: // start of line to cursor (blank them, keep length)
                    for (int i = 0; i < _cursor && i < _current.Length; i++)
                    {
                        _current[i] = ' ';
                    }

                    break;
                case 2: // entire line
                    _current.Clear();
                    _cursor = 0;
                    break;
            }
        }

        private static int ParseInt(string s, int fallback)
            => int.TryParse(s, out int v) ? v : fallback;
    }
}
