using System;
using System.IO;
using System.Windows.Forms;

namespace HanumanInstitute.CommonWpfApp
{
    public static class FileFolderDialog
    {
        public static string ShowFolderDialog(string defaultPath)
        {
            return ShowFolderDialog(defaultPath, true);
        }

        public static string ShowFolderDialog(string defaultPath, bool allowCreate)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.ShowNewFolderButton = allowCreate;
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    try
                    {
                        dlg.SelectedPath = Path.GetDirectoryName(defaultPath);
                    }
                    catch (PathTooLongException) { }
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return dlg.SelectedPath.EndsWith("\\", StringComparison.InvariantCulture) ? dlg.SelectedPath : dlg.SelectedPath + "\\";
                }
                return null;
            }
        }

        public static string ShowFileDialog(string defaultPath, string filter)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (!string.IsNullOrEmpty(defaultPath))
            {
                try
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
                }
                catch (PathTooLongException) { }
            }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true)
            {
                return dlg.FileName;
            }
            return null;
        }

        public static string[] ShowFileDialogMultiple(string defaultPath, string filter)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (!string.IsNullOrEmpty(defaultPath))
            {
                try
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
                }
                catch (PathTooLongException) { }
            }
            dlg.Filter = filter;
            dlg.Multiselect = true;
            if (dlg.ShowDialog().Value == true)
            {
                return dlg.FileNames;
            }
            return null;
        }

        public static string ShowSaveFileDialog(string defaultPath, string filter)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            if (!string.IsNullOrEmpty(defaultPath))
            {
                try
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(defaultPath);
                }
                catch (PathTooLongException) { }
            }
            dlg.Filter = filter;
            if (dlg.ShowDialog().Value == true)
            {
                return dlg.FileName;
            }
            return null;
        }
    }
}
