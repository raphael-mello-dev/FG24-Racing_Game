// Interface for states
public interface IState
{
    void OnStart();

    void OnUpdate();

    void OnEnd();
}

// Base state to be inherited by any state (game state or other)
public abstract class BaseState : IState
{
    public virtual void OnStart() { }

    public virtual void OnUpdate() { }

    public virtual void OnEnd() { }
}