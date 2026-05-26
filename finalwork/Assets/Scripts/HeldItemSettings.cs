using UnityEngine;

public class HeldItemSettings : MonoBehaviour
{
    [Header("Hand Transform")]
    public Vector3 holdPosition = Vector3.zero;
    public Vector3 holdRotation = Vector3.zero;
    public Vector3 holdScale = Vector3.one;

   [Header("Pedestal Placement")]
public Vector3 pedestalPositionOffset;
public Vector3 pedestalRotationOffset;
public Vector3 pedestalScale = Vector3.one;
}