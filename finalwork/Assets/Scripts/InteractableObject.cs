using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public int ownerPlayerIndex = 0;

    public abstract void Interact(PickupSystem player);
}