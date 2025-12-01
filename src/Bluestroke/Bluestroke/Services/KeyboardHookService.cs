using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Bluestroke.Services;

/// <summary>
/// Service for capturing keyboard input using Windows low-level keyboard hooks.
/// </summary>
public class KeyboardHookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private IntPtr _hookId = IntPtr.Zero;
    private readonly LowLevelKeyboardProc _proc;
    private bool _disposed;

    /// <summary>
    /// Event raised when a key is pressed.
    /// </summary>
    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    /// <summary>
    /// Initializes a new instance of the KeyboardHookService.
    /// </summary>
    public KeyboardHookService()
    {
        _proc = HookCallback;
    }

    /// <summary>
    /// Starts listening for keyboard events.
    /// </summary>
    /// <returns>True if the hook was successfully installed, false otherwise.</returns>
    public bool Start()
    {
        if (_hookId != IntPtr.Zero)
        {
            return true; // Already running
        }

        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;
        if (curModule != null)
        {
            _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            if (_hookId == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                System.Diagnostics.Debug.WriteLine($"Failed to install keyboard hook. Error code: {errorCode}");
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Stops listening for keyboard events.
    /// </summary>
    public void Stop()
    {
        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            OnKeyPressed(new KeyPressedEventArgs(vkCode));
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    /// <summary>
    /// Raises the KeyPressed event.
    /// </summary>
    protected virtual void OnKeyPressed(KeyPressedEventArgs e)
    {
        KeyPressed?.Invoke(this, e);
    }

    /// <summary>
    /// Disposes the keyboard hook.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the keyboard hook.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Stop();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer for KeyboardHookService.
    /// </summary>
    ~KeyboardHookService()
    {
        Dispose(false);
    }
}

/// <summary>
/// Event arguments for key press events.
/// </summary>
public class KeyPressedEventArgs : EventArgs
{
    /// <summary>
    /// The virtual key code of the pressed key.
    /// </summary>
    public int VirtualKeyCode { get; }

    /// <summary>
    /// Initializes a new instance of KeyPressedEventArgs.
    /// </summary>
    public KeyPressedEventArgs(int virtualKeyCode)
    {
        VirtualKeyCode = virtualKeyCode;
    }
}
