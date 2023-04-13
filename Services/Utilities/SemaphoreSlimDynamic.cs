using System.Threading;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Services;

/// <summary>
/// An extension of SemaphoreSlim that allows changing the capacity dynamically.
/// </summary>
public class SemaphoreSlimDynamic : SemaphoreSlim
{
    private readonly ReaderWriterLockSlim _lock;

    /// <summary>
    /// Gets the current capacity of the Semaphore.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Gets the absolute maximum capacity of the Semaphore.
    /// </summary>
    public int MaximumCapacity { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemaphoreSlimDynamic"/> class.
    /// </summary>
    /// <param name="initialCount">The initial number of slots.</param>
    /// <param name="maxCount">The maximum number of slots.</param>
    public SemaphoreSlimDynamic(int initialCount, int maxCount)
        : base(initialCount, maxCount)
    {
        // Note: Reducing capacity is done by invoking Wait() which may only complete when a semaphore is released.
        // This means that increasing it again may cause an overflow. To avoid this, we set the hard maximum to twice maxCount.

        if (initialCount < 1 || initialCount > maxCount) { throw new ArgumentOutOfRangeException(nameof(initialCount)); }

        _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        Capacity = initialCount;
        MaximumCapacity = maxCount;
    }

    /// <summary>
    /// Changes the capacity of the SemaphoreSlim to specified size.
    /// </summary>
    /// <param name="newSize">The new capacity of the SemaphoreSlim.</param>
    public void ChangeCapacity(int newSize)
    {
        newSize.CheckRange(nameof(newSize), min: 1, max: MaximumCapacity);

        var adjust = 0;
        if (newSize != Capacity)
        {
            _lock.EnterWriteLock();
            adjust = newSize - Capacity;
            Capacity = newSize;
            _lock.ExitWriteLock();
        }

        if (adjust > 0)
        {
            for (var i = 0; i < adjust; i++)
            {
                TryRelease();
            }
        }
        else if (adjust < 0)
        {
            for (var i = 0; i > adjust; i--)
            {
                _ = WaitAsync();
            }
        }
    }

    /// <summary>
    /// Tries to release a semaphore. This may fail in some rare cases if ChangeCapacity was called several time, in which case, it will return false and decrease CurrentSize.
    /// </summary>
    /// <returns>Whether the semaphore was successfully released.</returns>
    public bool TryRelease()
    {
        try
        {
            Release();
            return true;
        }
        catch (SemaphoreFullException)
        {
            // The semaphore size doesn't seem to be reduced in such case?
            //_lock.EnterWriteLock();
            //CurrentSize--;
            //_lock.ExitWriteLock();
            return false;
        }
    }
}
