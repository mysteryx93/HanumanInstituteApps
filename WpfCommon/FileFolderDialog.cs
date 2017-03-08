using System;
using System.IO;
using System.Windows.Forms;

namespace EmergenceGuardian.WpfCommon {
    public static class FileFolderDialog {
        public static string ShowFolderDialog(string defaultPath) {
            return ShowFolderDialog(defaultPath, true);
        }

        public static string ShowFolderDialog(string defaultPath, bool allowCreate) {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.ShowNewFolderButton = allowCreate;
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.SelectedPath = Path.GetDirectoryName(defaultPath);
            }
            catch { }
            if (dlg.ShowDialog() == DialogResult.OK) {
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

        public static string[] ShowFileDialogMultiple(string defaultPath, string filter) {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            try {
                if (!string.IsNullOrEmpty(defaultPath))
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
            }
            catch { }
            dlg.Filter = filter;
            dlg.Multiselect = true;
            if (dlg.ShowDialog().Value == true) {
                return dlg.FileNames;
            } else
                return null;
        }

        public static string ShowSaveFileDialog(string defaultPath, string filter) {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
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
