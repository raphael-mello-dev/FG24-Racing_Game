public class StateMachine
{
    // Reference for current state
    public IState currentState { get; private set; }

    public StateMachine() { }

    // Switching current state logic
    public void SwitchState<T>() where T : IState, new()
    {
        currentState?.OnEnd();
        currentState = new T();
        currentState.OnStart();
    }

    // OnUpdate logic
    public void OnUpdate() => currentState?.OnUpdate();
}