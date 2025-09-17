using UnityEngine;

namespace UndeadSurvivalGame.Gameplay
{ 
    [CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
    public class Item : ScriptableObject
    {
        public string ItemID;
        public string ItemName;
        public Sprite ItemIcon;
        public ItemType ItemType;
        public WeaponType WeaponType;
        public AmmoType AmmoType;
        public string Description;
        public bool IsStackable;
        public int MaxStack = 1;
        public GameObject WorldPrefab;
        public GameObject HandPrefab;
        public AudioClip PickupAllSound;
        public AudioClip PickupSomeSound;
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
}
