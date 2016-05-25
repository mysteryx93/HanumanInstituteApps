using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using MPC_API_LIB;

namespace Business {
    public class ProcessWatcher {
        private Timer timerKillIfFrozen;
        private MPC api;
        public bool AllowClose { get; set; }
        private bool isHandlingEvent;
        private int runAttemps;

        public ProcessWatcher(MPC api) {
            this.api = api;
            this.AllowClose = true;
            timerKillIfFrozen = new Timer(2000);
            timerKillIfFrozen.Elapsed += timerKillIfFrozen_Elapsed;
        }

        public void Start() {
            timerKillIfFrozen.Start();
        }

        public void Stop() {
            timerKillIfFrozen.Stop();
        }

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
