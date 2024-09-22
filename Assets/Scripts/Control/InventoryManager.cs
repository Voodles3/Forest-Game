using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System;
using Unity.Mathematics;

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
        [SerializeField] Transform dropOrigin;

        int activeIndex;

        PlayerActions actions;
        InputAction scrollAction;
        InputAction dropAction;
        float scrollInput;

        void Awake()
        {
            Singleton();
            InputSetup();
            activeItem = null; // TODO: make active item save between play sessions
            activeIndex = -1; 
        }

        void Singleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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
            int candidateIndex = activeIndex + direction;
            if (!CheckIndexExists(candidateIndex)) return;

            SetItemActive(candidateIndex);
        }

        public void AddItem(InventoryItem item)
        {
            if (inventory.Contains(item)) { throw new Exception("This item is already in your inventory!"); }

            inventory.Add(item);
            if (inventory.Count == 1) { SetItemActive(); }
        }

        void DropItem(int index)
        {
            if (!CheckIndexExists(index)) { Debug.Log("Your inventory is empty!"); return; }

            // Find which item is in this index
            // Remove it from the inventory
            // Activate the gameobject attached to it and throw it
            // Set a new item active

            InventoryItem itemToDrop = inventory[index];
            inventory.Remove(itemToDrop);

            itemToDrop.gameObject.SetActive(true); // Activate it first in order to access collider size

            Collider itemCollider = itemToDrop.gameObject.GetComponent<Collider>();
            Vector3 itemSize = itemCollider.bounds.size; // Check how large item is to make sure it spawns outside of player

            Vector3 spawnPosition = dropOrigin.position + (Camera.main.transform.forward * (itemSize.z / 2f + 0.5f));
            itemToDrop.transform.position = spawnPosition; // Spawn gObject at drop position
            
            Rigidbody itemRb = itemToDrop.gameObject.GetComponent<Rigidbody>();
            itemRb.velocity = GetComponent<Rigidbody>().velocity; // Make gObject inherit the player's current velocity

            Vector3 dropVector = Camera.main.transform.forward * dropForce; 
            itemRb.AddRelativeForce(dropVector, ForceMode.Impulse); // Throw gObject

            if (CheckIndexExists(index))
            {
                SetItemActive(index); // Set next item in inventory to active
            }
            else
            {
                SetItemActive(index - 1);
            }
        }

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

