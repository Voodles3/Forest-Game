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
        [SerializeField] List<InventoryItem> inventory = new();
        [SerializeField] InventoryItem activeItem;

        [Header("Fields")]
        [SerializeField] float dropForce;

        [Header("References")]
        [SerializeField] GameObject player;
        [SerializeField] Transform dropOrigin;
        [SerializeField] Transform facingDirection;
        Transform cameraTransform;

        int activeIndex;

        PlayerActions actions;
        InputAction scrollAction;
        InputAction dropAction;
        
        float scrollInput;

        void Awake()
        {
            Singleton();
            InputSetup();
            activeItem = null; // TODO: make active item + inventory save between play sessions
            activeIndex = -1;

            cameraTransform = Camera.main.transform;
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
            dropAction = actions.Gameplay.DropHeldItem;
        }

        void OnScroll(InputAction.CallbackContext ctx)
        {
            scrollInput = ctx.ReadValue<Vector2>().y;

            CycleActiveItem((scrollInput > 1)? 1 : -1); // 1 if scrollInput is greater than 1, else -1
        }

        void OnDrop(InputAction.CallbackContext ctx)
        {
            DropItem(activeIndex);
        }

        void CycleActiveItem(int direction)
        {
            int newIndex = activeIndex + direction;
            if (!CheckIndexExists(newIndex)) return;

            SetItemActive(newIndex);
        }

        public void AddItem(InventoryItem item)
        {
            if (inventory.Contains(item)) { throw new Exception("This item is already in your inventory!"); }

            inventory.Add(item);
            if (inventory.Count == 1) { SetItemActive(); }
        }

        #region Drop Item

        void DropItem(int index)
        {
            if (index == -1f) { Debug.Log("Your inventory is empty!"); return; }

            InventoryItem itemToDrop = inventory[index];
            inventory.Remove(itemToDrop);

            float offset = GetColliderSize(itemToDrop);
            
            Vector3 spawnPosition = SetSpawnPoint(offset);

            itemToDrop.gameObject.transform.position = spawnPosition;
            itemToDrop.gameObject.SetActive(true);
            
            ThrowItem(itemToDrop);

            SetItemActive(CheckIndexExists(index) ? index : index - 1); // Set next item in inventory to active
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

        void SetItemActive(int index=0)
        {
            if (!CheckIndexExists(index))
            {
                if (inventory.Any())
                {
                    activeItem = inventory[0];
                    activeIndex = 0;
                    throw new Exception($"There is no item at index {index}!");
                }
                activeItem = null; // Inventory is just empty
                activeIndex = -1;
                return;
            }

            activeItem = inventory[index];
            activeItem.Active = true;
            activeIndex = index;
        }

        bool CheckIndexExists(int index)
        {
            if (index >= 0 && index < inventory.Count)
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

