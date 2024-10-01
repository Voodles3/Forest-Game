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
        public bool canBePickedUp = true;
        public bool isHovering;

        static InventoryItem _currentlyActiveInstance;
        [SerializeField] bool _active;

        Renderer[] childRenderers;
        Color originalEmissionColor;

        public Color glowColor = Color.white;
        public float glowIntensity = 1.5f;

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

        void Awake()
        {
            childRenderers = GetComponentsInChildren<Renderer>();

            if (childRenderers.Length > 0 && childRenderers[0].material.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = childRenderers[0].material.GetColor("_EmissionColor");
            }
        }

        public void Interact()
        {
            // For now, the only interaction functionality for an InventoryItem is to be picked up
            if (canBePickedUp) { PickUp(); }
            else { Debug.Log("This item cannot be picked up!"); }
        }

        // public void OnHover(bool hovering)
        // {
        //     isHovering = hovering;

        //     if (isHovering)
        //     {
        //         foreach (Renderer renderer in childRenderers)
        //         {
        //             if (renderer.material.HasProperty("_EmissionColor"))
        //             {
        //                 renderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);
        //                 renderer.material.EnableKeyword("_EMISSION");
        //             }
        //         }
        //     }
        //     else
        //     {
        //         foreach (Renderer renderer in childRenderers)
        //         {
        //             if (renderer.material.HasProperty("_EmissionColor"))
        //             {
        //                 renderer.material.SetColor("_EmissionColor", originalEmissionColor);
        //                 renderer.material.DisableKeyword("_EMISSION"); // Turn off emission
        //             }
        //         }
        //     }
        // }

        void PickUp()
        {
            try
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            catch {}
            InventoryManager.Instance.AddItem(this);
            gameObject.SetActive(false);
        }
    }
}

