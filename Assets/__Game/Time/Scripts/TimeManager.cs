using UnityEngine;

/// <summary>FloatFactor를 사용하여 다중 타임스케일 배율을 곱으로 합산 관리하는 매니저</summary>
public class TimeManager : GlobalManagerBase
{
    public static TimeManager instance { get; private set; }
    #region Property
    /// <summary>등록된 모든 타임스케일 배율의 Factor</summary>
    public IReadOnlyFloatFactor TimeScaleFactor => m_TimeScaleFactor;
    #endregion
    #region Value
    private FloatFactor<MonoBehaviour> m_TimeScaleFactor;
    #endregion

    #region Event
    public override void InitSingleton()
    {
        instance = this;
        base.InitSingleton();
    }
    public override void Init()
    {
        m_TimeScaleFactor = new(this, FloatFactor<MonoBehaviour>.ETotalType.Multifly);
        TimeScaleFactor.AddChanged(this, OnUpdate, true);
        base.Init();
    }
    public override void InitGame()
    {
        OnUpdate(null);
        base.InitGame();
    }
    public override void OnShutdown()
    {
        Time.timeScale = 0;
        base.OnShutdown();
    }

    /// <summary>Factor 변경 시 Time.timeScale을 갱신</summary>
    private void OnUpdate(ValueBase _v)
    {
        Time.timeScale = TimeScaleFactor.Total;
    }
    #endregion
    #region Function
    /// <summary>지정된 소유자의 타임스케일 배율을 설정</summary>
    public void SetTimeScale(MonoBehaviour _callBy, MonoBehaviour _owner, float _value)
    {
        m_TimeScaleFactor.Set(_callBy, _owner, _value);
    }
    #endregion
}
