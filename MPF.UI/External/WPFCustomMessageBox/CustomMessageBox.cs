// -----------------------------------------------------------------------
// <copyright file="MessageBox.cs">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace WPFCustomMessageBox
{
    using System;
    using System.Windows;

    /// <summary>
    /// Displays a message box.
    /// </summary>
    public static class CustomMessageBox
    {
        /// <summary>
        /// Global parameter to enable (true) or disable (false) the removal of the title bar icon.
        /// If you are using a custom window style, the icon removal may cause issues like displaying two title bar (the default windows one and the custom one).
        /// </summary>
        private static readonly bool RemoveTitleBarIcon = true;

        /// <summary>
        /// Displays a message box that has a message and returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageBoxText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(null, messageBoxText, string.Empty, null, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message and a title bar caption; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(null, messageBoxText, caption, null, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message and returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(owner, messageBoxText, string.Empty, null, null, null, timeout, timeoutResult);

        }

        /// <summary>
        /// Displays a message box in front of the specified window. The message box displays a message and title bar caption; and it returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(owner, messageBoxText, caption, null, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and button; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
#pragma warning disable IDE0072
            return button switch
            {
                MessageBoxButton.YesNo
                    or MessageBoxButton.YesNoCancel => ShowYesNoMessage(null, messageBoxText, caption, null, null, null, null, timeout, timeoutResult),

                _ => ShowOKMessage(null, messageBoxText, caption, null, null, null, timeout, timeoutResult),
            };
#pragma warning restore IDE0072
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and button; and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
#pragma warning disable IDE0072
            return button switch
            {
                MessageBoxButton.YesNo
                    or MessageBoxButton.YesNoCancel => ShowYesNoMessage(owner, messageBoxText, caption, null, null, null, null, timeout, timeoutResult),

                _ => ShowOKMessage(owner, messageBoxText, caption, null, null, null, timeout, timeoutResult),
            };
#pragma warning restore IDE0072
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
#pragma warning disable IDE0072
            return button switch
            {
                MessageBoxButton.YesNo
                    or MessageBoxButton.YesNoCancel => ShowYesNoMessage(null, messageBoxText, caption, null, null, null, icon, timeout, timeoutResult),

                _ => ShowOKMessage(null, messageBoxText, caption, null, null, icon, timeout, timeoutResult),
            };
#pragma warning restore IDE0072
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, button, and icon; and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult Show(Window owner, string? messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
#pragma warning disable IDE0072
            return button switch
            {
                MessageBoxButton.YesNo
                    or MessageBoxButton.YesNoCancel => ShowYesNoMessage(owner, messageBoxText, caption, null, null, null, icon, timeout, timeoutResult),

                _ => ShowOKMessage(owner, messageBoxText, caption, null, null, icon, timeout, timeoutResult),
            };
#pragma warning restore IDE0072
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and OK button with a custom System.String value for the button's text; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(null, messageBoxText, caption, okButtonText, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, and OK button with a custom System.String value for the button's text; and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOK(Window owner, string messageBoxText, string caption, string okButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(owner, messageBoxText, caption, okButtonText, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, OK button with a custom System.String value for the button's text, and icon; and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOK(string messageBoxText, string caption, string okButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(null, messageBoxText, caption, okButtonText, null, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, title bar caption, OK button with a custom System.String value for the button's text, and icon; and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOK(Window owner, string messageBoxText, string caption, string okButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(owner, messageBoxText, caption, okButtonText, null, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and OK/Cancel buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(null, messageBoxText, caption, okButtonText, cancelButtonText, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and OK/Cancel buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOKCancel(Window owner, string messageBoxText, string caption, string okButtonText, string cancelButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(owner, messageBoxText, caption, okButtonText, cancelButtonText, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, OK/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOKCancel(string messageBoxText, string caption, string okButtonText, string cancelButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(null, messageBoxText, caption, okButtonText, cancelButtonText, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, OK/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowOKCancel(Window owner, string messageBoxText, string caption, string okButtonText, string cancelButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowOKMessage(owner, messageBoxText, caption, okButtonText, cancelButtonText, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and Yes/No buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(null, messageBoxText, caption, yesButtonText, noButtonText, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and Yes/No buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNo(Window owner, string messageBoxText, string caption, string yesButtonText, string noButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(owner, messageBoxText, caption, yesButtonText, noButtonText, null, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNo(string messageBoxText, string caption, string yesButtonText, string noButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(null, messageBoxText, caption, yesButtonText, noButtonText, null, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNo(Window owner, string messageBoxText, string caption, string yesButtonText, string noButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(owner, messageBoxText, caption, yesButtonText, noButtonText, null, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and Yes/No/Cancel buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(null, messageBoxText, caption, yesButtonText, noButtonText, cancelButtonText, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, and Yes/No/Cancel buttons with custom System.String values for the buttons' text;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNoCancel(Window owner, string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(owner, messageBoxText, caption, yesButtonText, noButtonText, cancelButtonText, null, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNoCancel(string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(null, messageBoxText, caption, yesButtonText, noButtonText, cancelButtonText, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">
        /// The message box will close automatically after <paramref name="timeout"/> milliseconds.
        /// If null, the message box will act like default message box and not close automatically.
        /// </param>
        /// <param name="timeoutResult">If the message box closes automatically due to the <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public static MessageBoxResult ShowYesNoCancel(Window owner, string messageBoxText, string caption, string yesButtonText, string noButtonText, string cancelButtonText, MessageBoxImage icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            return ShowYesNoMessage(owner, messageBoxText, caption, yesButtonText, noButtonText, cancelButtonText, icon, timeout, timeoutResult);
        }

        /// <summary>
        /// Displays a message box that has a message, caption, OK/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="okButtonText">A System.String that specifies the text to display within the OK button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">The message box will close automatically after given milliseconds</param>
        /// <param name="timeoutResult">If the message box closes automatically due to <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        private static MessageBoxResult ShowOKMessage(Window? owner, string? messageBoxText, string caption, string? okButtonText, string? cancelButtonText, MessageBoxImage? icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            MessageBoxButton buttonLayout = string.IsNullOrEmpty(cancelButtonText) ? MessageBoxButton.OK : MessageBoxButton.OKCancel;

            var msg = new CustomMessageBoxWindow(owner, messageBoxText, caption, buttonLayout, icon, RemoveTitleBarIcon);
            if (!string.IsNullOrEmpty(okButtonText))
                msg.OkButtonText = okButtonText;
            if (!string.IsNullOrEmpty(cancelButtonText))
                msg.CancelButtonText = cancelButtonText;

            ShowDialog(msg, timeout, timeoutResult);

            return msg.Result;
        }

        /// <summary>
        /// Displays a message box that has a message, caption, Yes/No/Cancel buttons with custom System.String values for the buttons' text, and icon;
        /// and that returns a result.
        /// </summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display.</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display.</param>
        /// <param name="yesButtonText">A System.String that specifies the text to display within the Yes button.</param>
        /// <param name="noButtonText">A System.String that specifies the text to display within the No button.</param>
        /// <param name="cancelButtonText">A System.String that specifies the text to display within the Cancel button.</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="timeout">The message box will close automatically after given milliseconds</param>
        /// <param name="timeoutResult">If the message box closes automatically due to <paramref name="timeout"/> being exceeded, this result is returned.</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        private static MessageBoxResult ShowYesNoMessage(Window? owner, string? messageBoxText, string caption, string? yesButtonText, string? noButtonText, string? cancelButtonText, MessageBoxImage? icon, int? timeout = null, MessageBoxResult timeoutResult = MessageBoxResult.None)
        {
            MessageBoxButton buttonLayout = string.IsNullOrEmpty(cancelButtonText) ? MessageBoxButton.YesNo : MessageBoxButton.YesNoCancel;

            var msg = new CustomMessageBoxWindow(owner, messageBoxText, caption, buttonLayout, icon, RemoveTitleBarIcon);
            if (!string.IsNullOrEmpty(yesButtonText))
                msg.YesButtonText = yesButtonText;
            if (!string.IsNullOrEmpty(noButtonText))
                msg.NoButtonText = noButtonText;
            if (!string.IsNullOrEmpty(cancelButtonText))
                msg.CancelButtonText = cancelButtonText;

            ShowDialog(msg, timeout, timeoutResult);

            return msg.Result;
        }

        private static void ShowDialog(CustomMessageBoxWindow dialog, int? timeout, MessageBoxResult timeoutResult)
        {
            if (timeout.HasValue && timeout.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), string.Format("Timeout must be greater than 0."));
            }

            if (timeout.HasValue)
            {
                //System.Threading.Timer timer = null;
                //timer = new System.Threading.Timer(s => { msg.Close(); timer.Dispose(); }, null, timeout.Value, System.Threading.Timeout.Infinite);
                var timer = new System.Timers.Timer(timeout.Value) { AutoReset = false };
                timer.Elapsed += delegate
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        dialog.Result = timeoutResult;
                        dialog.Close();
                        //timer.Stop();
                        //timer.Dispose();
                        //timer = null;
                    }));
                };
                timer.Start();
                dialog.ShowDialog();
                timer.Stop();
                timer.Dispose();
            }
            else
                dialog.ShowDialog();
        }
    }
}
