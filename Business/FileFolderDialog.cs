using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using System.Windows.Forms;

namespace Business {
    public static class FileFolderDialog {
        public static string ShowFolderDialog(string defaultPath) {
            return ShowFolderDialog(defaultPath, true);
        }

        public static string ShowFolderDialog(string defaultPath, bool allowCreate) {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = allowCreate;
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.SelectedPath = Path.GetDirectoryName(defaultPath);
            }
            catch { }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (dlg.SelectedPath.EndsWith("\\"))
                    return dlg.SelectedPath;
                else
                    return dlg.SelectedPath + "\\";
            } else
                return null;
        }

        public static string ShowFileDialog(string defaultPath, string filter) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
            }
            catch { }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true) {
                return dlg.FileName;
            } else
                return null;
        }
    }
}
