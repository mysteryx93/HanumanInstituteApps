using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32.SafeHandles;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Abstraction wrapper around the System.Diagnostics.Process class that can be used for unit testing.
    /// </summary>
    public class ProcessWrapper : IProcess, IDisposable
    {
        private readonly Process _process;

        public ProcessWrapper()
        {
            _process = new Process();
        }

        public ProcessWrapper(Process process)
        {
            _process = process;
        }

        public ProcessPriorityClass PriorityClass
        {
            get => _process.PriorityClass;
            set => _process.PriorityClass = value;
        }

        public bool PriorityBoostEnabled
        {
            get => _process.PriorityBoostEnabled;
            set => _process.PriorityBoostEnabled = value;
        }

        public long PeakVirtualMemorySize64 => _process.PeakVirtualMemorySize64;

        public long PeakWorkingSet64 => _process.PeakWorkingSet64;

        public long PeakPagedMemorySize64 => _process.PeakPagedMemorySize64;

        public long PagedMemorySize64 => _process.PagedMemorySize64;

        public long NonpagedSystemMemorySize64 => _process.NonpagedSystemMemorySize64;

        public ProcessModuleCollection Modules => _process.Modules;

        public IntPtr MinWorkingSet
        {
            get => _process.MinWorkingSet;
            set => _process.MinWorkingSet = value;
        }

        public long PagedSystemMemorySize64 => _process.PagedSystemMemorySize64;

        public long PrivateMemorySize64 => _process.PrivateMemorySize64;

        public TimeSpan PrivilegedProcessorTime => _process.PrivilegedProcessorTime;

        public string ProcessName => _process.ProcessName;

        public long WorkingSet64 => _process.WorkingSet64;

        public StreamReader StandardError => _process.StandardError;

        public StreamReader StandardOutput => _process.StandardOutput;

        public StreamWriter StandardInput => _process.StandardInput;

        public bool EnableRaisingEvents
        {
            get => _process.EnableRaisingEvents;
            set => _process.EnableRaisingEvents = value;
        }

        public long VirtualMemorySize64 => _process.VirtualMemorySize64;

        public TimeSpan UserProcessorTime => _process.UserProcessorTime;

        public TimeSpan TotalProcessorTime => _process.TotalProcessorTime;

        public ProcessThreadCollection Threads => _process.Threads;

        public ISynchronizeInvoke SynchronizingObject
        {
            get => _process.SynchronizingObject;
            set => _process.SynchronizingObject = value;
        }

        public DateTime StartTime => _process.StartTime;

        public ProcessStartInfo StartInfo
        {
            get => _process.StartInfo;
            set => _process.StartInfo = value;
        }

        public int SessionId => _process.SessionId;

        public bool Responding => _process.Responding;

        public IntPtr ProcessorAffinity
        {
            get => _process.ProcessorAffinity;
            set => _process.ProcessorAffinity = value;
        }

        public IntPtr MaxWorkingSet
        {
            get => _process.MaxWorkingSet;
            set => _process.MaxWorkingSet = value;
        }

        public ProcessModule MainModule => _process.MainModule;

        public string MainWindowTitle => _process.MainWindowTitle;

        public string MachineName => _process.MachineName;

        public int Id => _process.Id;

        public int HandleCount => _process.HandleCount;

        public SafeProcessHandle SafeHandle => _process.SafeHandle;

        public IntPtr Handle => _process.Handle;

        public DateTime ExitTime => _process.ExitTime;

        public bool HasExited => _process.HasExited;

        public int ExitCode => _process.ExitCode;

        public int BasePriority => _process.BasePriority;

        public IntPtr MainWindowHandle => _process.MainWindowHandle;

        public event DataReceivedEventHandler ErrorDataReceived
        {
            add => _process.ErrorDataReceived += value;
            remove => _process.ErrorDataReceived -= value;
        }

        public event DataReceivedEventHandler OutputDataReceived
        {
            add => _process.OutputDataReceived += value;
            remove => _process.OutputDataReceived -= value;
        }

        public event EventHandler Exited
        {
            add => _process.Exited += value;
            remove => _process.Exited -= value;
        }

        public void BeginErrorReadLine() => _process.BeginErrorReadLine();

        public void BeginOutputReadLine() => _process.BeginOutputReadLine();

        public void CancelErrorRead() => _process.CancelErrorRead();

        public void CancelOutputRead() => _process.CancelOutputRead();

        public void Close() => _process.Close();

        public bool CloseMainWindow() => _process.CloseMainWindow();

        public void Kill() => _process.Kill();

        public void Refresh() => throw new NotImplementedException();

        public bool Start() => _process.Start();

        public bool WaitForExit(int milliseconds) => _process.WaitForExit(milliseconds);

        public void WaitForExit() => _process.WaitForExit();

        public bool WaitForInputIdle(int milliseconds) => _process.WaitForInputIdle(milliseconds);

        public bool WaitForInputIdle() => _process.WaitForInputIdle();

        public override string ToString() => _process.ToString();

        private bool _disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _process.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
