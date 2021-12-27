// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Windows;
//

// namespace HanumanInstitute.CommonAvaloniaApp
// {
//     /// <summary>
//     /// Manages the display of windows.
//     /// By calling Show, the active window is hidden and replaced by another window.
//     /// By calling ShowToolbox, the window is shown as a toolbox that remains attached to whichever window is active.
//     /// </summary>
//     public class WindowManager
//     {
//         private readonly List<Window> _windowStack = new List<Window>();
//         private readonly List<Window> _toolboxes = new List<Window>();
//
//         /// <summary>
//         /// Initializes a new instance of the WindowManager class.
//         /// </summary>
//         /// <param name="rootWindow">The main window.</param>
//         public WindowManager(Window rootWindow)
//         {
//             if (rootWindow == null) { throw new ArgumentNullException(nameof(rootWindow)); }
//
//             _windowStack.Add(rootWindow);
//         }
//
//         /// <summary>
//         /// Returns the currently active window.
//         /// </summary>
//         public Window Current => _windowStack.LastOrDefault();
//
//         /// <summary>
//         /// Shows specified window while hiding the currently active window.
//         /// </summary>
//         /// <param name="newWindow">The new window to display.</param>
//         public void Show(Window newWindow)
//         {
//             if (newWindow == null) { throw new ArgumentNullException(nameof(newWindow)); }
//
//             var previousWindow = _windowStack.Last();
//             if (previousWindow != null)
//             {
//                 newWindow.Owner = previousWindow; // To position based on parent window.
//             }
//             newWindow.Show();
//             _windowStack.Add(newWindow);
//             if (previousWindow != null)
//             {
//                 previousWindow.Hide();
//             }
//             SetToolboxOwner(newWindow);
//             newWindow.Closing += NewWindow_Closed;
//         }
//
//         /// <summary>
//         /// Shows specified window in modal mode on top of the previous window.
//         /// </summary>
//         /// <param name="newWindow">The new window to display.</param>
//         public void ShowDialog(Window newWindow)
//         {
//             if (newWindow == null) { throw new ArgumentNullException(nameof(newWindow)); }
//
//             newWindow.Owner = Current;
//             newWindow.ShowDialog();
//         }
//
//         /// <summary>
//         /// Closes all windows except the main one.
//         /// </summary>
//         public void CloseToMain()
//         {
//             while (_windowStack.Count > 1)
//             {
//                 Current.Close();
//             }
//         }
//
//         /// <summary>
//         /// Occurs when the active window is closed. Shows the previous window in the stack.
//         /// </summary>
//         private void NewWindow_Closed(object sender, EventArgs e)
//         {
//             _windowStack.RemoveAt(_windowStack.Count - 1);
//             var previousWindow = _windowStack.LastOrDefault();
//             if (previousWindow != null)
//             {
//                 SetToolboxOwner(previousWindow);
//                 previousWindow.Show();
//             }
//         }
//
//         /// <summary>
//         /// Attaches a toolbox window to the active window. It will remain attached to whichever window is active.
//         /// </summary>
//         /// <param name="newToolbox">The toolbox window to attach.</param>
//         /// <param name="position">Where to position the toolbox window in relation to the active window.</param>
//         public void ShowToolbox(Window newToolbox, ToolboxPosition position)
//         {
//             if (newToolbox == null) { throw new ArgumentNullException(nameof(newToolbox)); }
//
//             newToolbox.Owner = Current;
//             if (position == ToolboxPosition.Right)
//             {
//                 newToolbox.Left = Current.Left + Current.Width;
//                 newToolbox.Top = Current.Top;
//             }
//             else if (position == ToolboxPosition.Bottom)
//             {
//                 newToolbox.Left = Current.Left;
//                 newToolbox.Top = Current.Top + Current.Height;
//             }
//             newToolbox.Show();
//             if (!_toolboxes.Contains(newToolbox))
//             {
//                 _toolboxes.Add(newToolbox);
//             }
//         }
//
//         /// <summary>
//         /// Sets specified owner for all toolbox windows.
//         /// </summary>
//         /// <param name="owner">The window that will own all toolbox windows.</param>
//         private void SetToolboxOwner(Window owner)
//         {
//             foreach (var item in _toolboxes)
//             {
//                 item.Owner = owner;
//             }
//         }
//     }
//
//     /// <summary>
//     /// Represents where to display a toolbox window in relation to the parent window.
//     /// </summary>
//     public enum ToolboxPosition
//     {
//         Right,
//         Bottom
//     }
// }
