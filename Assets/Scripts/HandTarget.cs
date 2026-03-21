using UnityEngine;

public abstract class HandTarget : MonoBehaviour
{
    public abstract void HandUse(Hand hand);
    public abstract Vector2 GetScreenPosition(Hand hand);
}
