using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Forest.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Inventory")]
        [SerializeField] InventoryItem[] inventory;
        [SerializeField] InventoryItem activeItem;

        [Header("Fields")]
        [SerializeField] float dropForce;

        [Header("References")]
        [SerializeField] GameObject player;
        [SerializeField] Transform dropOrigin;
        [SerializeField] Transform facingDirection;
        Transform cameraTransform;

        [SerializeField] int activeIndex;

        PlayerActions actions;
        InputAction scrollAction;
        InputAction dropAction;
        
        float scrollInput;

        void Awake()
        {
            Singleton();
            InputSetup();
            activeItem = null; // TODO: make active item + inventory save between play sessions
            activeIndex = 0;

            cameraTransform = Camera.main.transform;
        }

        void Start()
        {
            inventory = new InventoryItem[HotbarManager.Instance.HotbarSlots.Length];
        }

        void Singleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void InputSetup()
        {
            actions = new();
            scrollAction = actions.Gameplay.CycleInventory;
            dropAction = actions.Gameplay.DropItem;
        }

        void OnScroll(InputAction.CallbackContext ctx)
        {
            scrollInput = ctx.ReadValue<Vector2>().y;

            CycleActiveSlot((scrollInput > 1)? 1 : -1); // 1 if scrollInput is greater than 1, else -1
        }

        void OnDrop(InputAction.CallbackContext ctx)
        {
            DropItem(activeIndex);
        }

        void CycleActiveSlot(int direction)
        {
            int newIndex = activeIndex + direction;
            if (!CheckIndexExists(newIndex)) return;

            activeItem = inventory[newIndex];
            if (activeItem != null) { activeItem.Active = true; }
            activeIndex = newIndex;

            HotbarManager.Instance.HighlightHotbarSlot(newIndex);
        }

        public bool AddItem(InventoryItem item)
        {
            if (inventory.Contains(item)) 
            {
                Debug.LogWarning("This item is already in your inventory!");
                return false;
            }

            int nextAvailableSlot = Array.IndexOf(inventory, null);

            if (nextAvailableSlot == -1)
            {
                Debug.Log("Inventory is full!");
                HotbarManager.Instance.FlashRed(activeIndex);
                return false;
            }

            inventory[nextAvailableSlot] = item;
            if (activeIndex == nextAvailableSlot) { activeItem = item; }

            HotbarManager.Instance.SetSlotSprite(nextAvailableSlot, item.ItemSprite);
            return true;
        }


        #region Drop Item

        void DropItem(int index)
        {
            if (!CheckIndexExists(index) || inventory[index] == null)
            {
                Debug.Log("No item in this slot!");
                return;
            }

            InventoryItem itemToDrop = inventory[index];
            inventory[index] = null;
            activeItem = null;

            float offset = GetColliderSize(itemToDrop);
            
            Vector3 spawnPosition = SetSpawnPoint(offset);

            itemToDrop.gameObject.transform.position = spawnPosition;
            itemToDrop.gameObject.SetActive(true);
            
            ThrowItem(itemToDrop);
            
            HotbarManager.Instance.SetSlotSprite(index, null);
        }

        Vector3 SetSpawnPoint(float offset)
        {
            Vector3 spawnPosition = dropOrigin.position + (facingDirection.forward * offset);

            Vector3 rayDirection = (spawnPosition - cameraTransform.position).normalized;
            float rayDistance = Vector3.Distance(cameraTransform.position, spawnPosition);

            if (Physics.Raycast(cameraTransform.position, rayDirection, out RaycastHit hit, rayDistance))
            {
                return hit.point - rayDirection * offset;
            }

            return spawnPosition;
        }

        float GetColliderSize(InventoryItem itemToDrop)
        {
            itemToDrop.gameObject.SetActive(true); // Must be active to access collider size

            Collider itemCollider = itemToDrop.gameObject.GetComponent<Collider>();
            Vector3 itemSize = itemCollider.bounds.size;

            itemToDrop.gameObject.SetActive(false); // Must be inactive to teleport to spawnPosition

            return itemSize.magnitude / 2;
        }

        void ThrowItem(InventoryItem itemToDrop)
        {
            Rigidbody itemRb = itemToDrop.gameObject.GetComponent<Rigidbody>();

            itemRb.velocity = player.GetComponent<Rigidbody>().velocity; // Make gObject inherit the player's current velocity

            Vector3 dropVector = cameraTransform.forward * dropForce; 
            itemRb.AddForce(dropVector, ForceMode.Impulse); // Throw gObject
        }
        
        #endregion

        bool CheckIndexExists(int index)
        {
            if (index >= 0 && index < inventory.Length)
            {
                return true;
            }
            return false;
        }

        void OnEnable()
        {
            actions.Gameplay.Enable();
            scrollAction.performed += OnScroll;
            dropAction.started += OnDrop;
        }

        void OnDisable() 
        {
            actions.Gameplay.Disable();
            scrollAction.performed -= OnScroll;
            dropAction.started -= OnDrop;
        }
    }
}

