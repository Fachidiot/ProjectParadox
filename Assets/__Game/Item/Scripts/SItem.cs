using System;

/// <summary>보유 아이템 값. 개수와 최근 획득 시점 저장</summary>
[Serializable]
public struct SItem
{
    public int count;
    public long acquiredTicks;

    public DateTime AcquiredTime => new DateTime(acquiredTicks);

    public SItem(int _count, DateTime _time)
    {
        count = _count;
        acquiredTicks = _time.Ticks;
    }
}
