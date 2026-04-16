using UnityEngine;

public abstract class CharacterPhysicsBase : MonoBehaviour
{
    #region Type
    public enum EFlyState
    {
        None,
        Float,
        Jump,
        Fly
    }
    #endregion

    #region Event
    public virtual void Init()
    {
    }
    #endregion
}
