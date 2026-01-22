using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public string state = "Index";
    private Animator _animator;

    public void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetState(int value)
    {
        _animator.SetInteger(state, value);
    }
}
