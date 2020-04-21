using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;

namespace HanumanInstitute.CommonWpfApp
{
    public static class CommandHelper
    {
        public static ICommand InitCommand(ref ICommand cmd, Action execute) => InitCommand(ref cmd, execute, () => true);

        public static ICommand InitCommand(ref ICommand cmd, Action execute, Func<bool> canExecute)
        {
            return cmd ?? (cmd = new RelayCommand(execute, canExecute));
        }

        public static ICommand InitCommand<T>(ref ICommand cmd, Action<T> execute) => InitCommand<T>(ref cmd, execute, (t) => true);

        public static ICommand InitCommand<T>(ref ICommand cmd, Action<T> execute, Func<T, bool> canExecute)
        {
            return cmd ?? (cmd = new RelayCommand<T>(execute, canExecute));
        }
    }
}
