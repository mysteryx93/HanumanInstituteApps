using System;
using System.Drawing;
using System.Windows.Threading;
using GalaSoft.MvvmLight;

namespace HanumanInstitute.CommonWpfApp.ViewModels
{
    public class SplashViewModel : ViewModelBase
    {
        public event EventHandler? RequestShutdown;

        public SplashViewModel()
        {
            if (IsInDesignMode)
            {
                Image = new Bitmap(480, 480);
            }
        }

        public Bitmap? Image
        {
            get => _image;
            set
            {
                _image = value;
                RaisePropertyChanged(nameof(Image));
                RaisePropertyChanged(nameof(Size));
            }
        }
        private Bitmap? _image;

        public Size Size => _image?.Size ?? new Size();

        public bool IsLoadCompleted { get; private set; }

        public void LoadCompleted()
        {
            IsLoadCompleted = true;
            RaisePropertyChanged(nameof(IsLoadCompleted));
        }
    }
}
