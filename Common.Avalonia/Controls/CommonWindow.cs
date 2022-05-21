using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;

namespace HanumanInstitute.Common.Avalonia;

public abstract class CommonWindow<T> : CommonWindow
{
    public T ViewModel => (T)DataContext!;
}

public abstract class CommonWindow : Window
{
    protected CommonWindow()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Initialize();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    public override async void Show()
    {
        base.Show();
        await Task.Delay(1);
        SetWindowStartupLocationWorkaround();
    }
    
    protected abstract void Initialize();

    /// <summary>
    /// Sets a manual startup position that will be applied when the window is shown.
    /// </summary>
    /// <param name="position">The manual position to set.</param>
    public void SetManualStartupPosition(PixelPoint? position) => _manualPosition = position;
    private PixelPoint? _manualPosition;

    /// <summary>
    /// Fix center start position not working on Linux. 
    /// </summary>
    private void SetWindowStartupLocationWorkaround()
    {
        if (OperatingSystem.IsWindows())
        {
            // Not needed for Windows
            return;
        }

        var scale = PlatformImpl?.DesktopScaling ?? 1.0;
        var pOwner = Owner?.PlatformImpl;
        if (pOwner != null)
        {
            scale = pOwner.DesktopScaling;
        }
        var rect = new PixelRect(PixelPoint.Origin,
            PixelSize.FromSize(ClientSize, scale));
        if (_manualPosition != null)
        {
            Position = _manualPosition.Value;
        }
        else if (WindowStartupLocation == WindowStartupLocation.CenterScreen)
        {
            var screen = Screens.ScreenFromPoint(pOwner?.Position ?? Position);
            if (screen == null)
            {
                return;
            }
            Position = screen.WorkingArea.CenterRect(rect).Position;
        }
        else
        {
            if (pOwner == null ||
                WindowStartupLocation != WindowStartupLocation.CenterOwner)
            {
                return;
            }
            Position = new PixelRect(pOwner.Position,
                PixelSize.FromSize(pOwner.ClientSize, scale)).CenterRect(rect).Position;
        }
    }
}
