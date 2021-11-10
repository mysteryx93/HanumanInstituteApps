using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HanumanInstitute.MediaMuxer {
    /// <summary>
    /// Interaction logic for InitializingWindow.xaml
    /// </summary>
    public partial class WorkingWindow : Window {
        public async static Task InstanceAsync(Window owner, Task worker) {
            WorkingWindow W = new WorkingWindow();
            W.Owner = owner;

            CancellationToken token = new CancellationToken();
            Task InitWinTask = Task.Factory.StartNew(
                            () => W.ShowDialog(),
                            token,
                            TaskCreationOptions.None,
                            TaskScheduler.FromCurrentSynchronizationContext());

            worker.Start();
            await worker;

            W.Close();
            await InitWinTask;
        }

        // private WindowHelper helper;

        public WorkingWindow() {
            InitializeComponent();
            // helper = new WindowHelper(this);
        }
    }
}
