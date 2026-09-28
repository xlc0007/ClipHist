using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace ClipHist;

/// <summary>托盘应用上下文：维护托盘、剪贴板历史、快捷键与弹出面板。</summary>
internal sealed class MainApp : ApplicationContext
{
    private readonly NotifyIcon _tray;
    private readonly Icon _icon;
    private readonly ToolStripMenuItem _pauseItem;
    private readonly ClipboardHistory _history = new();
    private readonly ListenerWindow _listener;

    private PopupForm? _popup;
    private IntPtr _lastForeground = IntPtr.Zero;

    public MainApp()
    {
        _icon = MakeTrayIcon();

        _pauseItem = new ToolStripMenuItem("暂停记录");
        _pauseItem.Click += (_, _) => TogglePause();

        var exitItem = new ToolStripMenuItem("退出");
        exitItem.Click += (_, _) => ExitApp();

        var menu = new ContextMenuStrip();
        menu.Items.Add(_pauseItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exitItem);

        _tray = new NotifyIcon
        {
            Icon = _icon,
            Text = "剪贴板历史 (Ctrl+Shift+V)",
            Visible = true,
            ContextMenuStrip = menu,
        };
        _tray.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                ShowPopup();
        };

        _listener = new ListenerWindow(OnClipboardUpdate, ShowPopup);
        string? err = _listener.Start();
        if (err != null)
            Notify("提示", err);
    }

    private void OnClipboardUpdate()
    {
        if (_history.Paused)
            return;

        if (ClipboardHelper.TryReadUnicodeText(out string? text)
            && !string.IsNullOrEmpty(text))
        {
            _history.Add(text!);
        }
    }

    private void ShowPopup()
    {
        _lastForeground = NativeMethods.GetForegroundWindow();

        if (_popup == null || _popup.IsDisposed)
            _popup = new PopupForm(SelectItem);

        _popup.SetItems(_history.Items);
        NativeMethods.GetCursorPos(out NativeMethods.POINT pt);
        _popup.ShowNear(new Point(pt.X, pt.Y));
    }

    private void SelectItem(int index)
    {
        if (index < 0 || index >= _history.Count)
            return;

        string text = _history[index];
        IntPtr target = _lastForeground;

        ClosePopup();

        if (!ClipboardHelper.TryWriteText(text))
        {
            Notify("剪贴板写入失败", "内容未能写入剪贴板，请稍后重试。");
            return;
        }

        if (target != IntPtr.Zero)
            NativeMethods.SetForegroundWindow(target);

        Thread.Sleep(50);

        if (!SimulateCtrlV())
            Notify("自动粘贴失败", "内容已写入剪贴板，请手动 Ctrl+V 粘贴。");
    }

    private static bool SimulateCtrlV()
    {
        var inputs = new NativeMethods.INPUT[]
        {
            NativeMethods.MakeKeyInput(NativeMethods.VK_CONTROL, false),
            NativeMethods.MakeKeyInput(NativeMethods.VK_V, false),
            NativeMethods.MakeKeyInput(NativeMethods.VK_V, true),
            NativeMethods.MakeKeyInput(NativeMethods.VK_CONTROL, true),
        };
        uint inserted = NativeMethods.SendInput((uint)inputs.Length, inputs, System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.INPUT>());
        return inserted == inputs.Length;
    }

    private void TogglePause()
    {
        _history.Paused = !_history.Paused;
        _pauseItem.Checked = _history.Paused;
        _tray.Text = _history.Paused ? "剪贴板历史（已暂停）" : "剪贴板历史 (Ctrl+Shift+V)";
        Notify(
            _history.Paused ? "已暂停记录" : "已恢复记录",
            _history.Paused ? "暂停期间不新增或更新历史。" : "已继续记录剪贴板内容。");
    }

    private void ClosePopup()
    {
        if (_popup != null && !_popup.IsDisposed)
            _popup.Close();
    }

    private void Notify(string title, string message)
    {
        try
        {
            _tray.BalloonTipTitle = title;
            _tray.BalloonTipText = message;
            _tray.ShowBalloonTip(3000);
        }
        catch
        {
            // 托盘通知失败不影响运行
        }
    }

    private void ExitApp()
    {
        _listener.Stop();
        ClosePopup();
        _tray.Visible = false;
        _tray.Dispose();
        _icon.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _listener.Stop();
            ClosePopup();
            _tray.Visible = false;
            _tray.Dispose();
            _icon.Dispose();
        }
        base.Dispose(disposing);
    }

    private static Icon MakeTrayIcon()
    {
        using var bmp = new Bitmap(16, 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(Color.Transparent);

            using var b = new SolidBrush(Color.FromArgb(0x1F, 0x6F, 0xEB));
            g.FillRectangle(b, 3, 5, 10, 9);  // 主体
            g.FillRectangle(b, 6, 2, 4, 3);   // 夹子

            using var w = new SolidBrush(Color.White);
            g.FillRectangle(w, 5, 7, 6, 1);
            g.FillRectangle(w, 5, 9, 6, 1);
            g.FillRectangle(w, 5, 11, 4, 1);
        }

        IntPtr h = bmp.GetHicon();
        try
        {
            using var tmp = Icon.FromHandle(h);
            return (Icon)tmp.Clone();
        }
        finally
        {
            NativeMethods.DestroyIcon(h);
        }
    }
}
