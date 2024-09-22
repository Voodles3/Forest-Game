using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using Forest.Interaction;
using System;

namespace Forest.Inventory
{
    public class InventoryItem : MonoBehaviour, IInteractable
    {
        public string description;
        public bool canBePickedUp;

        static InventoryItem _currentlyActiveInstance;
        [SerializeField] bool _active;

        public bool Active // This property will ensure that only one InventoryItem will ever be active at a time
        {
            get { return _active; }
            set
            {
                if (value)
                {
                    // Deactivate the currently active instance (if any)
                    if (_currentlyActiveInstance != null && _currentlyActiveInstance != this)
                    {
                        _currentlyActiveInstance._active = false;
                    }
                    
                    // Set this instance as the active one
                    _currentlyActiveInstance = this;
                }

                // Update the active status for this instance
                _active = value;
            }
        }

        public InventoryItem()
        {
            _active = false;
        }

        public void Interact()
        {
            // For now the only interaction functionality for an InventoryItem is to be picked up
            if (canBePickedUp) { PickUp(); }
            else { Debug.Log("This item cannot be picked up!"); }
        }

        void PickUp()
        {
            gameObject.SetActive(false);
            InventoryManager.Instance.AddItem(this); 
        }
    }
}

