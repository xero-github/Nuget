﻿using System;
using System.ComponentModel.Composition;
using System.Windows.Threading;
using System.Windows.Media;

namespace NuGet.Dialog.PackageManagerUI {

    [Export(typeof(IProgressWindowOpener))]
    public sealed class ProgressWindowOpener : IProgressWindowOpener {
        private ProgressDialog _currentWindow;
        private readonly Dispatcher _uiDispatcher;

        public ProgressWindowOpener() {
            _uiDispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Show the progress window with the specified title.
        /// </summary>
        /// <param name="title">The window title</param>
        /// <remarks>
        /// This method can be called from worker thread.
        /// </remarks>
        public void Show(string title) {
            if (!_uiDispatcher.CheckAccess()) {
                // must use BeginInvoke() here to avoid blocking the worker thread
                _uiDispatcher.BeginInvoke(new Action<string>(Show), title);
                return;
            }

            if (IsOpen) {
                // if the window is hidden, just re-show it instead of creating a new window instance
                if (_currentWindow.Title != title) {
                    _currentWindow.Title = title;
                }
                _currentWindow.ShowDialog();
            }
            else {
                _currentWindow = new ProgressDialog();
                _currentWindow.Title = title;
                _currentWindow.Closed += OnWindowClosed;

                _currentWindow.ShowModal();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e) {
            if (_currentWindow != null) {
                _currentWindow.Closed -= OnWindowClosed;
                _currentWindow = null;
            }
        }

        /// <summary>
        /// Hide the progress window if it is open.
        /// </summary>
        /// <remarks>
        /// This method can be called from worker thread.
        /// </remarks>
        public void Hide() {
            if (!_uiDispatcher.CheckAccess()) {
                // must use BeginInvoke() here to avoid blocking the worker thread
                _uiDispatcher.BeginInvoke(new Action(Hide));
                return;
            }

            if (IsOpen) {
                _currentWindow.Hide();
            }
        }

        public bool IsOpen {
            get {
                return _currentWindow != null;
            }
        }

        public bool Close() {
            if (IsOpen) {
                _currentWindow.ForceClose();
                _currentWindow = null;
                return true;
            }
            else {
                return false;
            }
        }

        public void SetCompleted(bool successful) {
            if (IsOpen) {
                _currentWindow.SetCompleted(successful);
            }
        }

        /// <summary>
        /// Add a logging message to the progress window.
        /// </summary>
        /// <remarks>
        /// This method can be called from worker thread.
        /// </remarks>
        public void AddMessage(MessageLevel level, string message) {
            if (!_uiDispatcher.CheckAccess()) {
                _uiDispatcher.BeginInvoke(new Action<MessageLevel, string>(AddMessage), level, message);
                return;
            }

            if (IsOpen) {
                Brush messageBrush;

                // select message color based on MessageLevel value
                switch (level) {
                    case MessageLevel.Debug:
                        messageBrush = Brushes.DarkGray;
                        break;

                    case MessageLevel.Warning:
                        messageBrush = Brushes.Red;
                        break;

                    default:
                        messageBrush = Brushes.Black;
                        break;
                }

                _currentWindow.AddMessage(message, messageBrush);
            }
            else {
                throw new InvalidOperationException();
            }
        }
    }
}