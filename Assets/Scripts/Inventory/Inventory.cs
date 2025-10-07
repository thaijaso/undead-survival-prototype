using System;
using System.Collections.Generic;
using UndeadSurvivalGame.PlayerSystems;
using UndeadSurvivalGame.UI;
using UnityEngine;

namespace UndeadSurvivalGame.Gameplay
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        int capacity = 8;

        [SerializeReference]
        public ItemStack[] itemStacks;

        [SerializeField]
        private PlayerWeaponManager playerWeaponManager;

        public event Action OnInventoryChanged;

        public IReadOnlyList<ItemStack> ItemStacks => itemStacks;

        void Start()
        {
            SetupPlayerTransform();
            SetupItemStacks();
            OnInventoryChanged?.Invoke(); // Fire event so UI updates
        }
        
        private void SetupPlayerTransform()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    Debug.LogWarning("Player GameObject with tag 'Player' not found in the scene.");
                }
            }
        }

        private void SetupItemStacks()
        {
            if (itemStacks == null || itemStacks.Length != capacity)
            {
                itemStacks = new ItemStack[capacity];
            }

            // Clean up any Itemstacks with a null item (from inspector serialization)
            for (int index = 0; index < itemStacks.Length; index++)
            {
                if (itemStacks[index] != null && itemStacks[index].item == null)
                {
                    itemStacks[index] = null;
                }
            }
        }

        public bool IsEmpty()
        {
            if (itemStacks == null)
            {
                Debug.LogWarning("itemStacks is not initialized!");
                return true; // or false, depending on how you want to handle this case
            }

            bool allEmpty = true;

            foreach (var itemStack in itemStacks)
            {
                if (itemStack != null)
                {
                    allEmpty = false;
                    break;
                }
            }

            return allEmpty;
        }

        /// <summary>
        /// Attempts to add the specified quantity of an item to the inventory.
        /// Returns the number of items that could not be added (remaining quantity).
        /// 
        /// Parameters:
        /// - item: The Item to add.
        /// - quantity: The number of items to add (default is 1).
        /// 
        /// Return:
        /// - int: The quantity that could not be added.
        ///  0 means all items were added, positive means some remaining items were not added, 
        /// and negative means there was an error in item or quantity was invalid.
        /// </summary>
        public int TryAdd(Item item, int quantity = 1)
        {
            Debug.Log($"[{gameObject.name}] Inventory.TryAdd(): Attempting to add {quantity} of item '{item.name}'.");
            if (item == null || quantity <= 0)
            {
                Debug.LogWarning("Invalid item or quantity.");
                return quantity;
            }

            if (item.IsStackable)
            {
                quantity = AddStackableItem(item, quantity);
            }
            else
            {
                quantity = AddNonStackableItem(item, quantity);
            }

            return quantity;
        }

        /// <summary>
        /// Adds stackable items to existing stacks or creates new stacks.
        /// Returns the remaining quantity that could not be added.
        /// </summary>
        private int AddStackableItem(Item item, int quantity)
        {
            // Add to existing stacks
            for (int index = 0; index < itemStacks.Length; index++)
            {
                var itemStack = itemStacks[index];

                if (itemStack != null && itemStack.item.ItemID == item.ItemID)
                {
                    int spaceLeft = item.MaxStack - itemStack.quantity;
                    if (spaceLeft > 0)
                    {
                        int amountToAdd = Math.Min(spaceLeft, quantity);
                        itemStack.AddQuantity(amountToAdd);
                        quantity -= amountToAdd;
                        OnInventoryChanged?.Invoke();
                        if (quantity <= 0) break;
                    }
                }
            }

            // Add new stacks if there is still quantity left and there is capacity
            while (quantity > 0)
            {
                int indexToAdd = Array.FindIndex(itemStacks, stack => stack == null);

                if (indexToAdd != -1)
                {
                    int amountToAdd = Math.Min(quantity, item.MaxStack);
                    itemStacks[indexToAdd] = new ItemStack(item, amountToAdd);
                    quantity -= amountToAdd;
                    OnInventoryChanged?.Invoke();
                }
            }

            if (quantity > 0)
            {
                Debug.Log("Inventory is full. Some items could not be added.");
            }

            return quantity;
        }

        /// <summary>
        /// Adds non-stackable items to the inventory.
        /// Returns the remaining quantity that could not be added.
        /// </summary>
        private int AddNonStackableItem(Item item, int quantity)
        {
            if (itemStacks == null)
            {
                Debug.LogWarning("itemStacks is not initialized!");
                return quantity;
            }

            while (quantity > 0)
            {
                int indexToAdd = Array.FindIndex(itemStacks, stack => stack == null);

                if (indexToAdd != -1)
                {
                    itemStacks[indexToAdd] = new ItemStack(item, 1);
                    quantity--;
                    OnInventoryChanged?.Invoke();
                }
            }
           
            if (quantity > 0)
            {
                Debug.Log("Inventory is full. Some items could not be added.");
            }

            return quantity;
        }

        public int GetItemQuantity(string itemID)
        {
            if (itemStacks == null)
            {
                Debug.LogWarning("itemStacks is not initialized!");
                return 0;
            }

            int total = 0;

            foreach (var itemStack in itemStacks)
            {
                if (itemStack != null && itemStack.item.ItemID == itemID)
                {
                    total += itemStack.quantity;
                }
            }

            return total;
        }

        public int GetAmmoTypeQuantity(AmmoType ammoType)
        {
            if (itemStacks == null)
            {
                Debug.LogWarning("itemStacks is not initialized!");
                return 0;
            }

            int total = 0;

            foreach (var itemStack in itemStacks)
            {
                if (itemStack != null && itemStack.item.ItemType == ItemType.Ammo && itemStack.item.AmmoType == ammoType)
                {
                    total += itemStack.quantity;
                }
            }

            return total;
        }

        public void DecrementAmmo(AmmoType ammoType)
        {
            if (ammoType == AmmoType.None) return;

            for (int index = 0; index < itemStacks.Length; index++)
            {
                var itemStack = itemStacks[index];
                if (itemStack != null && itemStack.item.ItemType == ItemType.Ammo && itemStack.item.AmmoType == ammoType)
                {
                    itemStack.DecrementQuantity();

                    if (itemStack.IsEmpty)
                    {
                        itemStacks[index] = null;
                    }

                    OnInventoryChanged?.Invoke();
                    return;
                }
            }

            Debug.LogWarning($"[{gameObject.name}] Inventory.DecrementAmmo(): No ammo of type {ammoType} found.");
        }

        public void RemoveAmmo(AmmoType ammoType, int amount)
        {
            if (ammoType == AmmoType.None || amount <= 0) return;

            for (int index = 0; index < itemStacks.Length; index++)
            {
                var itemStack = itemStacks[index];
                if (itemStack != null && itemStack.item.ItemType == ItemType.Ammo && itemStack.item.AmmoType == ammoType)
                {
                    int quantityBeforeRemoval = itemStack.quantity;
                    itemStack.RemoveQuantity(amount);
                    int quantityAfterRemoval = itemStack.quantity;
                    amount -= quantityBeforeRemoval - quantityAfterRemoval;

                    if (itemStack.IsEmpty)
                    {
                        itemStacks[index] = null;
                    }

                    OnInventoryChanged?.Invoke();
                }
            }

            if (amount > 0)
            {
                Debug.LogWarning($"[{gameObject.name}] Inventory.RemoveAmmo(): Not enough ammo of type {ammoType} to remove the requested amount.");
            }
        }

        public void DropItemStack(int index)
        {
            if (index < 0 || index >= itemStacks.Length)
            {
                Debug.LogWarning("Invalid index to drop item stack.");
                return;
            }

            if (itemStacks == null)
            {
                Debug.LogWarning("itemStacks is not initialized!");
                return;
            }

            ItemStack itemStackToDrop = itemStacks[index];

            // If the dropped item was the currently equipped weapon, clear it from the weapon manager
            if (playerWeaponManager != null && playerWeaponManager.IsItemEquipped(itemStackToDrop.item))
            {
                playerWeaponManager.UnequipCurrentWeapon();
            }

            SpawnDroppedItem(itemStackToDrop);
            itemStacks[index] = null;
            OnInventoryChanged?.Invoke();
        }

        private void SpawnDroppedItem(ItemStack itemStack)
        {
            if (itemStack == null || itemStack.item == null || playerTransform == null)
            {
                Debug.LogWarning("Cannot spawn dropped item. ItemStack, Item, or PlayerTransform is null.");
                return;
            }

            var itemsGameObject = GameObject.Find("Items");
            Quaternion prefabRotation = itemStack.item.WorldPrefab.transform.rotation;
            GameObject droppedItemObj;

            if (itemsGameObject == null)
            {
                Debug.LogWarning("No 'Items' GameObject found in the scene. Dropped item will be instantiated at root level.");
                droppedItemObj = Instantiate(itemStack.item.WorldPrefab, playerTransform.position, prefabRotation);
            }
            else
            {
                droppedItemObj = Instantiate(
                    itemStack.item.WorldPrefab,
                    playerTransform.position,
                    prefabRotation,
                    itemsGameObject.transform
                );
            }

            droppedItemObj.GetComponent<Collider>().enabled = true;
            droppedItemObj.GetComponent<ProximityUI>().enabled = true;

            ItemPickupInteractable itemPickup = droppedItemObj.GetComponent<ItemPickupInteractable>();
            itemPickup.Initialize(itemStack.item, itemStack.quantity);
            itemPickup.enabled = true;
        }

        public Item GetFirstWeapon()
        {
            if (IsEmpty())
            {
                Debug.LogWarning("Inventory.GetFirstWeapon() - Inventory is empty.");
                return null;
            }

            foreach (var itemStack in itemStacks)
            {
                if (itemStack.item.ItemType == ItemType.Weapon)
                {
                    return itemStack.item;
                }
            }

            Debug.LogWarning("No weapon found in inventory.");
            return null;
        }
    }
}
