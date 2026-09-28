using System;
using System.Windows.Forms;

namespace ClipHist;

/// <summary>仅消息窗口：注册剪贴板监听与全局快捷键。</summary>
internal sealed class ListenerWindow : NativeWindow
{
    private const int HotKeyId = 0x9A1;
    private static readonly IntPtr HwndMessage = new IntPtr(-3);

    private readonly Action _onClipboardUpdate;
    private readonly Action _onHotkey;

    public ListenerWindow(Action onClipboardUpdate, Action onHotkey)
    {
        _onClipboardUpdate = onClipboardUpdate;
        _onHotkey = onHotkey;
    }

    /// <summary>创建窗口并注册监听/快捷键。返回 null 表示成功，否则返回错误描述。</summary>
    public string? Start()
    {
        CreateHandle(new CreateParams { Parent = HwndMessage });
        NativeMethods.AddClipboardFormatListener(Handle);

        bool ok = NativeMethods.RegisterHotKey(
            Handle,
            HotKeyId,
            NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT,
            NativeMethods.VK_V);

        if (!ok)
            return "快捷键 Ctrl+Shift+V 注册失败，可能已被其他程序占用。可点击托盘图标打开历史列表。";

        return null;
    }

    public void Stop()
    {
        if (Handle != IntPtr.Zero)
        {
            NativeMethods.UnregisterHotKey(Handle, HotKeyId);
            NativeMethods.RemoveClipboardFormatListener(Handle);
            DestroyHandle();
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
        {
            _onClipboardUpdate();
        }
        else if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt64() == HotKeyId)
        {
            _onHotkey();
        }

        base.WndProc(ref m);
    }
}
