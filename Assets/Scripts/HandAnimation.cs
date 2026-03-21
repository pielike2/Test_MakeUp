using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HandAnimation : MonoBehaviour
{    
    public enum Type { brush, lipstick, eyeshadows}

    private Animator animator;
    private Action eventOnClipEnd;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    //«апустить анимацию инструмента
    public void RunAnimation(Type type, Action actionOnClipEnd)
    {
        switch (type)
        {
            case Type.brush:
                animator.Play("Brush");
                eventOnClipEnd += actionOnClipEnd;
                break;
            case Type.lipstick:
                animator.Play("Lipstick");
                eventOnClipEnd += actionOnClipEnd;
                break;
            case Type.eyeshadows:
                animator.Play("EyeShadows");
                eventOnClipEnd += actionOnClipEnd;
                break;
            default:
                break;
        };
    }

    //»вент срабатывает в конце любой из анимаций выше
    public void OnClipEnd()
    {
        eventOnClipEnd?.Invoke();
        eventOnClipEnd = null;
    }
}
