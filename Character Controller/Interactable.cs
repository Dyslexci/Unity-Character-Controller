using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public enum InteractionType
    {
        Click,
        Hold
    }

    public InteractionType interactionType;
    float holdTime;

    public abstract string GetDescription();
    public abstract void Interact(FirstPersonMovementController controller);

    public void IncreaseHoldTime() => holdTime += Time.deltaTime;
    public void ResetHoldTime() => holdTime = 0;
    public float GetHoldTime() => holdTime;
}
