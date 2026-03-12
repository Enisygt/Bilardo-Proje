using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ClientApplication.Services;

public class KioskService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    public event Action? AdminPanelRequested;

    public KioskService()
    {
        _proc = HookCallback;
    }

    public void StartKioskMode()
    {
        _hookID = SetHook(_proc);
    }

    public void StopKioskMode()
    {
        if (_hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using Process curProcess = Process.GetCurrentProcess();
        using ProcessModule curModule = curProcess.MainModule!;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName!), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam);
            Key key = KeyInterop.KeyFromVirtualKey(vkCode);

            bool alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

            if (ctrl && alt && shift && key == Key.F12)
            {
                AdminPanelRequested?.Invoke();
                return (IntPtr)1;
            }

            if (key == Key.LWin || key == Key.RWin) return (IntPtr)1;
            if (alt && key == Key.Tab) return (IntPtr)1;
            if (alt && key == Key.F4) return (IntPtr)1;
            if (ctrl && key == Key.Escape) return (IntPtr)1;
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    public void Dispose()
    {
        StopKioskMode();
    }
}
