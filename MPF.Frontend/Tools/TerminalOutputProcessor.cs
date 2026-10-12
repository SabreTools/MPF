using System;
using System.Collections.Generic;
using System.Text;

namespace MPF.Frontend.Tools
{
    /// <summary>
    /// Minimal terminal output processor
    /// </summary>
    public sealed class TerminalOutputProcessor
    {
        #region Properties

        /// <summary>
        /// Lines committed so far
        /// </summary>
        /// <remarks>Each line was terminated by <c>\n</c>)</remarks>
        public readonly List<string> CommittedLines = [];

        /// <summary>
        /// The live line being assembled after the last <c>\n</c>
        /// </summary>
        public string CurrentLine => _current.ToString();

        /// <summary>
        /// Raised once per committed line
        /// </summary>
        public event Action<string>? LineCommitted;

        /// <summary>
        /// Raised when the live line changes (printable char, <c>\r</c> overwrite, erase)
        /// </summary>
        public event Action<string>? CurrentLineChanged;

        #endregion

        #region Private Fields

        /// <summary>
        /// Current internal state
        /// </summary>
        private readonly StringBuilder _current = new();

        /// <summary>
        /// Write column within <see cref="_current"/>
        /// </summary>
        private int _cursor;

        /// <summary>
        /// Mid ANSI escape sequence (survives chunk boundaries)
        /// </summary>
        private bool _inEscape;

        /// <summary>
        /// Reusable current escape sequence
        /// </summary>
        private StringBuilder _escape = new();

        #endregion

        #region Private Enumerations

        /// <summary>
        /// Erase mode for inline edits
        /// </summary>
        private enum EraseMode
        {
            /// <summary>
            /// Cursor to end of line
            /// </summary>
            EndOfLine = 0,

            /// <summary>
            /// Start of line to cursor (blank them, keep length)
            /// </summary>
            StartToCursor = 1,

            /// <summary>
            /// Entire line
            /// </summary>
            EntireLine = 2,
        }

        #endregion

        /// <summary>
        /// Force-commit whatever is in the live line (e.g. on process exit)
        /// </summary>
        public void Flush()
        {
            if (_current.Length > 0)
                CommitLine();
        }

        /// <summary>
        /// Process the next chunk of input, saving internal state
        /// </summary>
        /// <param name="chunk">Chunk of input to process</param>
        public void Process(string? chunk)
        {
            if (string.IsNullOrEmpty(chunk))
                return;

            bool liveChanged = false;
            for (int i = 0; i < chunk!.Length; i++)
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
#if NET20 || NET35
                        _escape = new StringBuilder();
#else
                        _escape.Clear();
#endif
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

        #region Helpers

        /// <summary>
        /// Apply a CSI sequence given its parameter string and final byte
        /// </summary>
        /// <param name="parameters">Parameter string representing the sequence</param>
        /// <param name="final">Final byte of the sequence</param>
        /// <returns>Whether the CSI sequence was applied</returns>
        private bool ApplyCSISequence(string parameters, char final)
        {
            switch (final)
            {
                // EL -- erase in line
                case 'K':
                    EraseInLine((EraseMode)ParseIntWithFallback(parameters, 0));
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
        /// Commit a complete line to the output
        /// </summary>
        private void CommitLine()
        {
            string line = _current.ToString();
            CommittedLines.Add(line);
            LineCommitted?.Invoke(line);

#if NET20 || NET35
            _escape = new StringBuilder();
#else
            _escape.Clear();
#endif
            _cursor = 0;
        }

        /// <summary>
        /// Erase characters inside the current line
        /// </summary>
        /// <param name="mode">Mode determining how to erase characters</param>
        private void EraseInLine(EraseMode mode)
        {
            switch (mode)
            {
                case EraseMode.EndOfLine:
                    if (_cursor < _current.Length)
                        _current.Length = _cursor;

                    break;

                case EraseMode.StartToCursor:
                    for (int i = 0; i < _cursor && i < _current.Length; i++)
                    {
                        _current[i] = ' ';
                    }

                    break;

                // Entire line
                case EraseMode.EntireLine:
#if NET20 || NET35
                    _escape = new StringBuilder();
#else
                    _escape.Clear();
#endif
                    _cursor = 0;
                    break;

                // Ignore all other mode values
                default:
                    break;
            }
        }

        /// <summary>
        /// Consume one character of an in-progress escape sequence
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Whether the live line changed</returns>
        private bool FeedEscape(char value)
        {
            // First char after ESC decides the family.
            if (_escape.Length == 0)
            {
                if (value == '[')
                {
                    _escape.Append(value); // CSI -- keep collecting until a final byte
                    return false;
                }

                // Not a CSI (e.g. ESC( charset, ESC] OSC). We don't model these;
                // drop ESC + this byte and resume. Good enough for tool output.
                _inEscape = false;
                return false;
            }

            // Inside CSI: parameters/intermediates until a final byte 0x40-0x7E.
            if (value >= '@' && value <= '~')
            {
                bool changed = ApplyCSISequence(_escape.ToString(1, _escape.Length - 1), value);
                _inEscape = false;
#if NET20 || NET35
                _escape = new StringBuilder();
#else
                _escape.Clear();
#endif
                return changed;
            }

            _escape.Append(value);
            return false;
        }

        /// <summary>
        /// Try to parse an integer value, including a fallback if it fails
        /// </summary>
        /// <param name="value">Value to try to parse</param>
        /// <param name="fallback">Fallback value if parsing fails</param>
        /// <returns>The parsed value on success, the fallback value on error</returns>
        private static int ParseIntWithFallback(string value, int fallback)
            => int.TryParse(value, out int v) ? v : fallback;

        /// <summary>
        /// Write a single character to the current output
        /// </summary>
        /// <param name="value">Character to write to the output</param>
        private void WriteChar(char value)
        {
            if (_cursor < _current.Length)
                _current[_cursor] = value;
            else
                _current.Append(value);

            _cursor++;
        }

        #endregion
    }
}
