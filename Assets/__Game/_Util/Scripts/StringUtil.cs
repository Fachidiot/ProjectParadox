using UnityEngine;

/// <summary>문자열 포맷팅 확장 메서드 제공</summary>
public static class StringUtil
{
    #region Value
    private static readonly string[] m_Units = { "", "K", "M", "B", "T", "P", "E" };
    #endregion

    #region Function
    /// <summary>long을 단위 접미사 포함 문자열로 변환 (예: 10000 → 10.0K)</summary>
    public static string ToStringLong(this long _number, bool _isUnit)
    {
        if (_isUnit)
        {
            if (_number < 10000)
                return _number.ToString("N0");

            int unitIndex = (int)(Mathf.Log10(_number) / 3);
            unitIndex = Mathf.Min(unitIndex, m_Units.Length - 1);

            double value = _number / Mathf.Pow(1000, unitIndex);

            return $"{value:F1}{m_Units[unitIndex]}";
        }
        else
            return _number.ToString("N0");
    }
    #endregion
}
