using System;

namespace MPF.Core.Utilities
{
    /// <summary>
    /// Methods to deal with outputting tones to the PC speaker
    /// </summary>
    public class Chime
    {
        /// <summary>
        /// Standard duration to play a single tone
        /// </summary>
        private const int standardDurationMs = 200;

        #region Octave 0

        /// <summary>
        /// Frequency representing C(0)
        /// </summary>
        private const int noteC0 = 16; // 16.35

        /// <summary>
        /// Frequency representing D(0)
        /// </summary>
        private const int noteD0 = 18; // 18.35

        /// <summary>
        /// Frequency representing E(0)
        /// </summary>
        private const int noteE0 = 21; // 20.60

        /// <summary>
        /// Frequency representing F(0)
        /// </summary>
        private const int noteF0 = 22; // 21.83

        /// <summary>
        /// Frequency representing G(0)
        /// </summary>
        private const int noteG0 = 25; // 24.50

        /// <summary>
        /// Frequency representing A(0)
        /// </summary>
        private const int noteA0 = 28; // 27.50

        /// <summary>
        /// Frequency representing B(0)
        /// </summary>
        private const int noteB0 = 31; // 30.87

        #endregion

        #region Octave 1

        /// <summary>
        /// Frequency representing C(1)
        /// </summary>
        private const int noteC1 = 33; // 32.70

        /// <summary>
        /// Frequency representing D(1)
        /// </summary>
        private const int noteD1 = 37; // 36.71

        /// <summary>
        /// Frequency representing E(1)
        /// </summary>
        private const int noteE1 = 41; // 41.20

        /// <summary>
        /// Frequency representing F(1)
        /// </summary>
        private const int noteF1 = 44; // 43.65

        /// <summary>
        /// Frequency representing G(1)
        /// </summary>
        private const int noteG1 = 49; // 49.00

        /// <summary>
        /// Frequency representing A(1)
        /// </summary>
        private const int noteA1 = 55; // 55.00

        /// <summary>
        /// Frequency representing B(1)
        /// </summary>
        private const int noteB1 = 62; // 61.74

        #endregion

        #region Octave 2

        /// <summary>
        /// Frequency representing C(2)
        /// </summary>
        private const int noteC2 = 65; // 65.41

        /// <summary>
        /// Frequency representing D(2)
        /// </summary>
        private const int noteD2 = 73; // 73.42

        /// <summary>
        /// Frequency representing E(2)
        /// </summary>
        private const int noteE2 = 82; // 82.41

        /// <summary>
        /// Frequency representing F(2)
        /// </summary>
        private const int noteF2 = 87; // 87.31

        /// <summary>
        /// Frequency representing G(2)
        /// </summary>
        private const int noteG2 = 98; // 98.00

        /// <summary>
        /// Frequency representing A(2)
        /// </summary>
        private const int noteA2 = 110; // 110.00

        /// <summary>
        /// Frequency representing B(2)
        /// </summary>
        private const int noteB2 = 123; // 123.47

        #endregion

        #region Octave 3

        /// <summary>
        /// Frequency representing C(3)
        /// </summary>
        private const int noteC3 = 131; // 130.81

        /// <summary>
        /// Frequency representing D(3)
        /// </summary>
        private const int noteD3 = 147; // 146.83

        /// <summary>
        /// Frequency representing E(3)
        /// </summary>
        private const int noteE3 = 165; // 164.81

        /// <summary>
        /// Frequency representing F(3)
        /// </summary>
        private const int noteF3 = 175; // 174.61

        /// <summary>
        /// Frequency representing G(3)
        /// </summary>
        private const int noteG3 = 196; // 196.00

        /// <summary>
        /// Frequency representing A(3)
        /// </summary>
        private const int noteA3 = 220; // 220.00

        /// <summary>
        /// Frequency representing B(3)
        /// </summary>
        private const int noteB3 = 247; // 246.94

        #endregion

        #region Octave 4

        /// <summary>
        /// Frequency representing C(4)
        /// </summary>
        private const int noteC4 = 262; // 261.63

        /// <summary>
        /// Frequency representing D(4)
        /// </summary>
        private const int noteD4 = 294; // 293.66

        /// <summary>
        /// Frequency representing E(4)
        /// </summary>
        private const int noteE4 = 330; // 329.63

        /// <summary>
        /// Frequency representing F(4)
        /// </summary>
        private const int noteF4 = 349; // 349.23

        /// <summary>
        /// Frequency representing G(4)
        /// </summary>
        private const int noteG4 = 392; // 392.00

        /// <summary>
        /// Frequency representing A(4)
        /// </summary>
        private const int noteA4 = 440; // 440.00

        /// <summary>
        /// Frequency representing B(4)
        /// </summary>
        private const int noteB4 = 494; // 493.88

        #endregion

        #region Octave 5

        /// <summary>
        /// Frequency representing C(5)
        /// </summary>
        private const int noteC5 = 523; // 523.25

        /// <summary>
        /// Frequency representing D(5)
        /// </summary>
        private const int noteD5 = 587; // 587.33

        /// <summary>
        /// Frequency representing E(5)
        /// </summary>
        private const int noteE5 = 659; // 659.25

        /// <summary>
        /// Frequency representing F(5)
        /// </summary>
        private const int noteF5 = 698; // 698.46

        /// <summary>
        /// Frequency representing G(5)
        /// </summary>
        private const int noteG5 = 783; // 783.99

        /// <summary>
        /// Frequency representing A(5)
        /// </summary>
        private const int noteA5 = 880; // 880.00

        /// <summary>
        /// Frequency representing B(5)
        /// </summary>
        private const int noteB5 = 988; // 987.77

        #endregion

        #region Octave 6

        /// <summary>
        /// Frequency representing C(6)
        /// </summary>
        private const int noteC6 = 1047; // 1046.50

        /// <summary>
        /// Frequency representing D(6)
        /// </summary>
        private const int noteD6 = 1175; // 1174.66

        /// <summary>
        /// Frequency representing E(6)
        /// </summary>
        private const int noteE6 = 1319; // 1318.51

        /// <summary>
        /// Frequency representing F(6)
        /// </summary>
        private const int noteF6 = 1397; // 1396.91

        /// <summary>
        /// Frequency representing G(6)
        /// </summary>
        private const int noteG6 = 1568; // 1567.98

        /// <summary>
        /// Frequency representing A(6)
        /// </summary>
        private const int noteA6 = 1760; // 1760.00

        /// <summary>
        /// Frequency representing B(6)
        /// </summary>
        private const int noteB6 = 1976; // 1975.53

        #endregion

        #region Octave 7

        /// <summary>
        /// Frequency representing C(7)
        /// </summary>
        private const int noteC7 = 2093; // 2093.00

        /// <summary>
        /// Frequency representing D(7)
        /// </summary>
        private const int noteD7 = 2349; // 2349.32

        /// <summary>
        /// Frequency representing E(7)
        /// </summary>
        private const int noteE7 = 2637; // 2637.02

        /// <summary>
        /// Frequency representing F(7)
        /// </summary>
        private const int noteF7 = 2794; // 2793.83

        /// <summary>
        /// Frequency representing G(7)
        /// </summary>
        private const int noteG7 = 3136; // 3135.96

        /// <summary>
        /// Frequency representing A(7)
        /// </summary>
        private const int noteA7 = 3520; // 3520.00

        /// <summary>
        /// Frequency representing B(7)
        /// </summary>
        private const int noteB7 = 3951; // 3951.07

        #endregion

        #region Octave 8

        /// <summary>
        /// Frequency representing C(8)
        /// </summary>
        private const int noteC8 = 4186; // 4186.01

        /// <summary>
        /// Frequency representing D(8)
        /// </summary>
        private const int noteD8 = 4699; // 4698.63

        /// <summary>
        /// Frequency representing E(8)
        /// </summary>
        private const int noteE8 = 5274; // 5274.04

        /// <summary>
        /// Frequency representing F(8)
        /// </summary>
        private const int noteF8 = 5588; // 5587.65

        /// <summary>
        /// Frequency representing G(8)
        /// </summary>
        private const int noteG8 = 6272; // 6271.93

        /// <summary>
        /// Frequency representing A(8)
        /// </summary>
        private const int noteA8 = 7040; // 7040.00

        /// <summary>
        /// Frequency representing B(8)
        /// </summary>
        private const int noteB8 = 7902; // 7902.13

        #endregion

        /// <summary>
        /// Output a series of beeps for completion, similar to DiscImageCreator
        /// </summary>
        /// <param name="success">True if the upward series should play, false otherwise</param>
        public static void StandardCompletion(bool success)
        {
            if (success)
            {
                Console.Beep(noteC4, standardDurationMs);
                Console.Beep(noteD4, standardDurationMs);
                Console.Beep(noteE4, standardDurationMs);
                Console.Beep(noteF4, standardDurationMs);
                Console.Beep(noteG4, standardDurationMs);
                Console.Beep(noteA4, standardDurationMs);
                Console.Beep(noteB4, standardDurationMs);
                Console.Beep(noteC5, standardDurationMs);
            }
            else
            {
                Console.Beep(noteC5, standardDurationMs);
                Console.Beep(noteB4, standardDurationMs);
                Console.Beep(noteA4, standardDurationMs);
                Console.Beep(noteG4, standardDurationMs);
                Console.Beep(noteF4, standardDurationMs);
                Console.Beep(noteE4, standardDurationMs);
                Console.Beep(noteD4, standardDurationMs);
                Console.Beep(noteC4, standardDurationMs);
            }
        }
    }
}
