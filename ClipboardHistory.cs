using System.Collections.Generic;

namespace ClipHist;

internal sealed class ClipboardHistory
{
    public const int MaxCount = 5;

    private readonly List<string> _items = new();

    public bool Paused { get; set; }

    public int Count => _items.Count;

    public IReadOnlyList<string> Items => _items;

    public string this[int index] => _items[index];

    /// <summary>新增一条记录；重复内容移动到首位不占新槽位。返回是否发生变化。</summary>
    public bool Add(string text)
    {
        int idx = _items.IndexOf(text);
        if (idx == 0)
            return false; // 已是最新
        if (idx > 0)
            _items.RemoveAt(idx);

        _items.Insert(0, text);
        if (_items.Count > MaxCount)
            _items.RemoveAt(_items.Count - 1);
        return true;
    }
}
