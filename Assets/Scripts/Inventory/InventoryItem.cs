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
            // For now, the only interaction functionality for an InventoryItem is to be picked up
            if (canBePickedUp) { PickUp(); }
            else { Debug.Log("This item cannot be picked up!"); }
        }

        void PickUp()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            InventoryManager.Instance.AddItem(this);
            gameObject.SetActive(false);
        }
    }
}

