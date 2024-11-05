using UnityEngine;
using UnityEngine.UI;

namespace Forest.UI
{
    public class HotbarSlot : MonoBehaviour
    {
        static HotbarSlot _currentlyActiveSlot;
        [SerializeField] bool _active;

        public Image overlayImage;
        public Image slotImage;

        public bool Active // This property will ensure that only one HotbarSlot will ever be active at a time
        {
            get => _active;
            set
            {
                if (value)
                {
                    // Deactivate the currently active instance (if any)
                    if (_currentlyActiveSlot != null && _currentlyActiveSlot != this)
                    {
                        _currentlyActiveSlot.Active = false;
                    }
                    
                    // Set this instance as the active one
                    _currentlyActiveSlot = this;
                }

                // Update the active status for this instance
                _active = value;
                overlayImage.enabled = value;
            }
        }

        void Awake()
        {
            slotImage = GetComponent<Image>();
        }
    }
}




