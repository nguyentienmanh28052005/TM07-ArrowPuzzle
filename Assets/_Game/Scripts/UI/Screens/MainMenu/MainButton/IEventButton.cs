using UnityEngine;

public interface IEventButton
{
    public abstract Vector2 GetScreenPosition();
    public abstract bool CanStartFlyJump();
    public abstract void StartFlyJump();
    public abstract void CancelFlyJump();
    public abstract void Animate();
    public abstract void Register();
    public abstract void UnRegister();
}
