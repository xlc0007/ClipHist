using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClipHist;

internal static class ClipboardHelper
{
    private const int MaxAttempts = 5;
    private const int RetryDelayMs = 30;

    public static bool TryReadUnicodeText(out string? text)
    {
        for (int i = 0; i < MaxAttempts; i++)
        {
            try
            {
                if (Clipboard.ContainsText(TextDataFormat.UnicodeText))
                {
                    text = Clipboard.GetText(TextDataFormat.UnicodeText);
                    if (text != null)
                        return true;
                }
                text = null;
                return false;
            }
            catch (ExternalException)
            {
                // 剪贴板被其他程序暂时占用，短暂重试
                Thread.Sleep(RetryDelayMs);
            }
        }
        text = null;
        return false;
    }

    public static bool TryWriteText(string text)
    {
        for (int i = 0; i < MaxAttempts; i++)
        {
            try
            {
                Clipboard.SetText(text, TextDataFormat.UnicodeText);
                return true;
            }
            catch (ExternalException)
            {
                Thread.Sleep(RetryDelayMs);
            }
        }
        return false;
    }
}
