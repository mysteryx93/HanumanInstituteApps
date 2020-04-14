using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HanumanInstitute.CommonWpfApp {
    /// <summary>
    /// Manages the display of windows.
    /// By calling Show, the active window is hidden and replaced by another window.
    /// By calling ShowToolbox, the window is shown as a toolbox that remains attached to whichever window is active.
    /// </summary>
    public class WindowManager {
        private List<Window> windowStack = new List<Window>();
        private List<Window> toolboxes = new List<Window>();

        /// <summary>
        /// Initializes a new instance of the WindowManager class.
        /// </summary>
        /// <param name="rootWindow">The main window.</param>
        public WindowManager(Window rootWindow) {
            windowStack.Add(rootWindow);
        }

        /// <summary>
        /// Returns the currently active window.
        /// </summary>
        public Window Current {
            get { 
                return windowStack.LastOrDefault(); 
            }
        }

        /// <summary>
        /// Shows specified window while hiding the currently active window.
        /// </summary>
        /// <param name="newWindow">The new window to display.</param>
        public void Show(Window newWindow) {
            Window PreviousWindow = windowStack.Last();
            if (PreviousWindow != null) 
                newWindow.Owner = PreviousWindow; // To position based on parent window.
            newWindow.Show();
            windowStack.Add(newWindow);
            if (PreviousWindow != null)
                PreviousWindow.Hide();
            SetToolboxOwner(newWindow);
            newWindow.Closing += newWindow_Closed;
        }

        /// <summary>
        /// Shows specified window in modal mode on top of the previous window.
        /// </summary>
        /// <param name="newWindow">The new window to display.</param>
        public void ShowDialog(Window newWindow) {
            newWindow.Owner = Current;
            newWindow.ShowDialog();
        }

        /// <summary>
        /// Closes all windows except the main one.
        /// </summary>
        public void CloseToMain() {
            while (windowStack.Count > 1) {
                Current.Close();
            }
        }

        /// <summary>
        /// Occurs when the active window is closed. Shows the previous window in the stack.
        /// </summary>
        private void newWindow_Closed(object sender, EventArgs e) {
            windowStack.RemoveAt(windowStack.Count() - 1);
            Window PreviousWindow = windowStack.LastOrDefault();
            if (PreviousWindow != null) {
                SetToolboxOwner(PreviousWindow);
                PreviousWindow.Show();
            }
        }

        /// <summary>
        /// Attaches a toolbox window to the active window. It will remain attached to whichever window is active.
        /// </summary>
        /// <param name="newToolbox">The toolbox window to attach.</param>
        /// <param name="position">Where to position the toolbox window in relation to the active window.</param>
        public void ShowToolbox(Window newToolbox, ToolboxPosition position) {
            newToolbox.Owner = Current;
            if (position == ToolboxPosition.Right) {
                newToolbox.Left = Current.Left + Current.Width;
                newToolbox.Top = Current.Top;
            } else if (position == ToolboxPosition.Bottom) {
                newToolbox.Left = Current.Left;
                newToolbox.Top = Current.Top + Current.Height;
            }
            newToolbox.Show();
            if (!toolboxes.Contains(newToolbox))
                toolboxes.Add(newToolbox);
        }

        /// <summary>
        /// Sets specified owner for all toolbox windows.
        /// </summary>
        /// <param name="owner">The window that will own all toolbox windows.</param>
        private void SetToolboxOwner(Window owner) {
            foreach (Window item in toolboxes) {
                item.Owner = owner;
            }
        }
    }

    /// <summary>
    /// Represents where to display a toolbox window in relation to the parent window.
    /// </summary>
    public enum ToolboxPosition {
        Right,
        Bottom
    }
}
