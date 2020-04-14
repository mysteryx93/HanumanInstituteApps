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
        private Process process;

        public ProcessWrapper()
        {
            process = new Process();
        }

        public ProcessWrapper(Process process)
        {
            this.process = process;
        }

        public ProcessPriorityClass PriorityClass {
            get => process.PriorityClass;
            set => process.PriorityClass = value;
        }

        public bool PriorityBoostEnabled {
            get => process.PriorityBoostEnabled;
            set => process.PriorityBoostEnabled = value;
        }

        public long PeakVirtualMemorySize64 => process.PeakVirtualMemorySize64;

        public long PeakWorkingSet64 => process.PeakWorkingSet64;

        public long PeakPagedMemorySize64 => process.PeakPagedMemorySize64;

        public long PagedMemorySize64 => process.PagedMemorySize64;

        public long NonpagedSystemMemorySize64 => process.NonpagedSystemMemorySize64;

        public ProcessModuleCollection Modules => process.Modules;

        public IntPtr MinWorkingSet {
            get => process.MinWorkingSet;
            set => process.MinWorkingSet = value;
        }

        public long PagedSystemMemorySize64 => process.PagedSystemMemorySize64;

        public long PrivateMemorySize64 => process.PrivateMemorySize64;

        public TimeSpan PrivilegedProcessorTime => process.PrivilegedProcessorTime;

        public string ProcessName => process.ProcessName;

        public long WorkingSet64 => process.WorkingSet64;

        public StreamReader StandardError => process.StandardError;

        public StreamReader StandardOutput => process.StandardOutput;

        public StreamWriter StandardInput => process.StandardInput;

        public bool EnableRaisingEvents {
            get => process.EnableRaisingEvents;
            set => process.EnableRaisingEvents = value;
        }

        public long VirtualMemorySize64 => process.VirtualMemorySize64;

        public TimeSpan UserProcessorTime => process.UserProcessorTime;

        public TimeSpan TotalProcessorTime => process.TotalProcessorTime;

        public ProcessThreadCollection Threads => process.Threads;

        public ISynchronizeInvoke SynchronizingObject {
            get => process.SynchronizingObject;
            set => process.SynchronizingObject = value;
        }

        public DateTime StartTime => process.StartTime;

        public ProcessStartInfo StartInfo {
            get => process.StartInfo;
            set => process.StartInfo = value;
        }

        public int SessionId => process.SessionId;

        public bool Responding => process.Responding;

        public IntPtr ProcessorAffinity {
            get => process.ProcessorAffinity;
            set => process.ProcessorAffinity = value;
        }

        public IntPtr MaxWorkingSet {
            get => process.MaxWorkingSet;
            set => process.MaxWorkingSet = value;
        }

        public ProcessModule MainModule => process.MainModule;

        public string MainWindowTitle => process.MainWindowTitle;

        public string MachineName => process.MachineName;

        public int Id => process.Id;

        public int HandleCount => process.HandleCount;

        public SafeProcessHandle SafeHandle => process.SafeHandle;

        public IntPtr Handle => process.Handle;

        public DateTime ExitTime => process.ExitTime;

        public bool HasExited => process.HasExited;

        public int ExitCode => process.ExitCode;

        public int BasePriority => process.BasePriority;

        public IntPtr MainWindowHandle => process.MainWindowHandle;

        public event DataReceivedEventHandler ErrorDataReceived {
            add => process.ErrorDataReceived += value;
            remove => process.ErrorDataReceived -= value;
        }

        public event DataReceivedEventHandler OutputDataReceived {
            add => process.OutputDataReceived += value;
            remove => process.OutputDataReceived -= value;
        }

        public event EventHandler Exited {
            add => process.Exited += value;
            remove => process.Exited -= value;
        }

        public void BeginErrorReadLine()
        {
            process.BeginErrorReadLine();
        }

        public void BeginOutputReadLine()
        {
            process.BeginOutputReadLine();
        }

        public void CancelErrorRead()
        {
            process.CancelErrorRead();
        }

        public void CancelOutputRead()
        {
            process.CancelOutputRead();
        }

        public void Close()
        {
            process.Close();
        }

        public bool CloseMainWindow()
        {
            return process.CloseMainWindow();
        }

        public void Kill()
        {
            process.Kill();
        }

        public void Refresh()
        {
            throw new NotImplementedException();
        }

        public bool Start()
        {
            return process.Start();
        }

        public bool WaitForExit(int milliseconds)
        {
            return process.WaitForExit(milliseconds);
        }

        public void WaitForExit()
        {
            process.WaitForExit();
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            return process.WaitForInputIdle(milliseconds);
        }

        public bool WaitForInputIdle()
        {
            return process.WaitForInputIdle();
        }

        public override string ToString()
        {
            return process.ToString();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) 
                return;

            if (disposing)
            {
                process.Dispose();
            }

            disposed = true;
        }
    }
}
