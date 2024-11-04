using UnityEngine;
using Forest.Interaction;

namespace Forest.Inventory
{
    [RequireComponent(typeof(HoverEffect))]
    public class InventoryItem : MonoBehaviour, IInteractable
    {
        [SerializeField] bool _active;
        static InventoryItem _currentlyActiveInstance;

        [SerializeField] Sprite itemSprite;
        public Sprite ItemSprite => itemSprite;


        HoverEffect hoverEffect;
        public string description;
        public bool canBePickedUp = true;

        public bool Active // This property will ensure that only one InventoryItem will ever be active at a time
        {
            get => _active;
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
            hoverEffect = GetComponent<HoverEffect>();
        }

        public void Interact()
        {
            // For now, the only interaction functionality for an InventoryItem is to be picked up
            if (canBePickedUp) { PickUp(); }
            else { Debug.Log("This item cannot be picked up!"); }
        }

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
            hoverEffect.StopHoverEffect();
        }
    }
}