using Avalonia;
using Avalonia.Controls;

namespace HanumanInstitute.Common.Avalonia;

public abstract class CommonWindow<T> : Window
{
    protected CommonWindow()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Initialize();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected abstract void Initialize();

    public T ViewModel => (T)DataContext!;
}
