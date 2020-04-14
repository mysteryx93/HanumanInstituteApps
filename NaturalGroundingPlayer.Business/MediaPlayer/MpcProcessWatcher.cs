using System;
using System.Timers;
using MPC_API_LIB;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Pings the MPC-HC process regularly to kill and restart the process if it stops responding
    /// </summary>
    public interface IMpcProcessWatcher {
        /// <summary>
        /// Gets or sets whether the user is allowed to close the window. If not, it will respawn it when closed.
        /// </summary>
        bool AllowClose { get; set; }
        /// <summary>
        /// Start watching the process.
        /// </summary>
        void Start();
        /// <summary>
        /// Stop watching the process.
        /// </summary>
        void Stop();
        /// <summary>
        /// Ensures the MPC-HC process is running, and starts it if not running.
        /// </summary>
        void EnsureRunning();
    }

    #endregion

    /// <summary>
    /// Pings the MPC-HC process regularly to kill and restart the process if it stops responding
    /// </summary>
    public class MpcProcessWatcher : IMpcProcessWatcher {

        #region Declarations / Constructors

        /// <summary>
        /// Gets or sets whether the user is allowed to close the window. If not, it will respawn it when closed.
        /// </summary>
        public bool AllowClose { get; set; } = true;
        private Timer timerKillIfFrozen;
        private bool isHandlingEvent;
        private int runAttemps;

        private MPC api;

        public MpcProcessWatcher() : this(new MPC()) { }

        public MpcProcessWatcher(MPC api) {
            this.api = api;
            timerKillIfFrozen = new Timer(2000);
            timerKillIfFrozen.Elapsed += timerKillIfFrozen_Elapsed;
        }

        #endregion

        /// <summary>
        /// Start watching the process.
        /// </summary>
        public void Start() {
            timerKillIfFrozen.Start();
        }

        /// <summary>
        /// Stop watching the process.
        /// </summary>
        public void Stop() {
            timerKillIfFrozen.Stop();
        }

        /// <summary>
        /// Pings the process and kill/restart the process if necessary.
        /// </summary>
        private void timerKillIfFrozen_Elapsed(object sender, ElapsedEventArgs e) {
            if (isHandlingEvent)
                return;

            timerKillIfFrozen.Interval = 2000;
            isHandlingEvent = true;
            if (api.MpcProcess != null && !api.MpcProcess.HasExited && !api.MpcProcess.Responding) {
                bool Responding = false;
                for (int i = 0; i < 6; i++) {
                    System.Threading.Thread.Sleep(200);
                    if (api.MpcProcess.Responding) {
                        Responding = true;
                        break;
                    }
                }

                if (!Responding) {
                    try {
                        api.MpcProcess.Kill();
                    }
                    catch {
                    }
                    api.MpcProcess = null;
                    System.Threading.Thread.Sleep(500);
                }
            }

            if (api.MpcProcess != null && !api.MpcProcess.HasExited)
                runAttemps = 0;

            if (!AllowClose)
                EnsureRunning();

            isHandlingEvent = false;
        }

        /// <summary>
        /// Ensures the MPC-HC process is running, and starts it if not running.
        /// </summary>
        public void EnsureRunning() {
            if (!timerKillIfFrozen.Enabled)
                timerKillIfFrozen.Start();

            if ((api.MpcProcess == null || api.MpcProcess.HasExited) && runAttemps < 3) {
                runAttemps++;
                timerKillIfFrozen.Interval = 3000 + 3000 * runAttemps;
                api.Run();
            }
        }
    }
}
