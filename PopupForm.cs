using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClipHist;

/// <summary>无边框历史列表面板。</summary>
internal sealed class PopupForm : Form
{
    private const int ItemWidth = 324;
    private const int ItemHeight = 26;
    private const int ItemMargin = 2;
    private const int ClientWidthPx = ItemWidth + 12;
    private const int ClientPadV = 6;
    private const int SummaryMaxChars = 48;

    private readonly Action<int> _onSelect;
    private readonly FlowLayoutPanel _panel;
    private int _count;

    public PopupForm(Action<int> onSelect)
    {
        _onSelect = onSelect;

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.White;
        Padding = Padding.Empty;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowIcon = false;

        _panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = false,
            Padding = new Padding(6),
            Margin = Padding.Empty,
            BackColor = Color.White,
        };
        Controls.Add(_panel);
    }

    public void SetItems(IReadOnlyList<string>? items)
    {
        _panel.Controls.Clear();
        int n = (items == null || items.Count == 0) ? 1 : items.Count;
        _count = items?.Count ?? 0;

        if (_count == 0)
        {
            _panel.Controls.Add(MakeHint());
        }
        else
        {
            for (int i = 0; i < items!.Count; i++)
                _panel.Controls.Add(MakeItem(i, items[i]));
        }

        ClientSize = new Size(ClientWidthPx, n * (ItemHeight + ItemMargin * 2) + ClientPadV * 2);
    }

    public void ShowNear(Point pt)
    {
        var screen = Screen.PrimaryScreen?.WorkingArea ?? Screen.FromPoint(pt).WorkingArea;

        int x = pt.X;
        int y = pt.Y;
        if (x + Width > screen.Right) x = screen.Right - Width - 4;
        if (y + Height > screen.Bottom) y = screen.Bottom - Height - 4;
        if (x < screen.Left) x = screen.Left + 4;
        if (y < screen.Top) y = screen.Top + 4;

        Location = new Point(x, y);
        Show();
        Activate();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        switch (keyData)
        {
            case Keys.D1:
            case Keys.NumPad1: Select(0); return true;
            case Keys.D2:
            case Keys.NumPad2: Select(1); return true;
            case Keys.D3:
            case Keys.NumPad3: Select(2); return true;
            case Keys.D4:
            case Keys.NumPad4: Select(3); return true;
            case Keys.D5:
            case Keys.NumPad5: Select(4); return true;
            case Keys.Escape: Close(); return true;
        }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        Close();
    }

    private void Select(int index)
    {
        if (index < 0 || index >= _count)
            return;
        _onSelect(index);
    }

    private Control MakeItem(int index, string text)
    {
        var lbl = new Label
        {
            Text = $"{index + 1}. {Summarize(text)}",
            AutoSize = false,
            Width = ItemWidth,
            Height = ItemHeight,
            Margin = new Padding(ItemMargin),
            Padding = new Padding(6, 0, 6, 0),
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Microsoft YaHei UI", 10f, FontStyle.Regular),
            Cursor = Cursors.Hand,
            BackColor = Color.White,
        };
        lbl.Click += (_, _) => Select(index);
        lbl.MouseEnter += (_, _) => lbl.BackColor = Color.FromArgb(230, 240, 255);
        lbl.MouseLeave += (_, _) => lbl.BackColor = Color.White;
        return lbl;
    }

    private static Control MakeHint()
    {
        return new Label
        {
            Text = "（暂无剪贴板记录）",
            AutoSize = false,
            Width = ItemWidth,
            Height = ItemHeight,
            Margin = new Padding(ItemMargin),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Microsoft YaHei UI", 9f, FontStyle.Regular),
            ForeColor = Color.Gray,
            BackColor = Color.White,
        };
    }

    /// <summary>单行摘要：换行/制表符转空格、连续空白折叠、超长截断。</summary>
    private static string Summarize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);
        bool lastSpace = false;
        foreach (char c in text)
        {
            bool isSpace = c == ' ' || c == '\t' || c == '\r' || c == '\n';
            if (isSpace)
            {
                if (!lastSpace)
                    sb.Append(' ');
                lastSpace = true;
            }
            else
            {
                sb.Append(c);
                lastSpace = false;
            }
        }

        string s = sb.ToString().Trim();
        return s.Length > SummaryMaxChars ? s.Substring(0, SummaryMaxChars) + "…" : s;
    }
}
