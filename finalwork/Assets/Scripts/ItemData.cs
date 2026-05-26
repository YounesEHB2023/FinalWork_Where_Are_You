using UnityEngine;

public enum GreekWeaponType
{
    None,
    Trident,
    Bow,
    SpearShield,
    Lightning
}

public class ItemData : MonoBehaviour
{
    public Sprite icon;

    [Header("Greek Puzzle")]
    public GreekWeaponType greekWeaponType;
}