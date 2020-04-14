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
using System.Windows.Navigation;
using System.Windows.Shapes;
using EmergenceGuardian.NaturalGroundingPlayer.Business;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace NaturalGroundingPlayer {
    public partial class SetupWizardPage4 : Page {
        public SetupWizardPage4() {
            InitializeComponent();
        }

        private SetupWizard owner;

        private async void Page_Loaded(object sender, RoutedEventArgs e) {
            owner = (SetupWizard)Window.GetWindow(this);

            MpcConfiguration.IsSvpEnabled = true;
            MpcConfiguration.IsMadvrEnabled = true;
            MpcConfiguration.IsWidescreenEnabled = false;
            await owner.DownloadAndPlaySample("Ellie Goulding", "Burn");
        }
    }
}
