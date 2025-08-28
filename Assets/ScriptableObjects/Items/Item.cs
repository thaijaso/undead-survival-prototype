using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{
    public string itemID;
    public string itemName;
    public Sprite itemIcon;
    public ItemType itemType;
    public WeaponType weaponType;
    public AmmoType ammoType;
    public string description;
    public bool isStackable;
    public int maxStack = 1;
    public GameObject prefab;
    public AudioClip pickupAllSound;
    public AudioClip pickupSomeSound;
}

public enum ItemType
{
    Consumable,
    Weapon,
    Material,
    Clothing,
    Ammo
}

public enum WeaponType
{
    None,
    Handgun,
    Shotgun,
    Rifle,
    Melee
}

public enum AmmoType
{
    None,
    Magnum44,
    NineMM
}
