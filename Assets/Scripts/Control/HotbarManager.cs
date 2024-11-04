using System;
using UnityEngine;
using UnityEngine.UI;

namespace Forest.Inventory
{
    public class HotbarManager : MonoBehaviour
    {
        public static HotbarManager Instance { get; private set; }

        [SerializeField] Image[] hotbarSlots;
        public Image[] HotbarSlots => hotbarSlots;

        void Awake()
        {
            Singleton();
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

        public void SetHotbarSlot(int slotIndex, Sprite itemSprite)
        {   
            if (!CheckIndexExists(slotIndex)) { return; }

            hotbarSlots[slotIndex].sprite = itemSprite;
            hotbarSlots[slotIndex].enabled = itemSprite != null;
            hotbarSlots[slotIndex].SetNativeSize();
        }

        public void HighlightHotbarSlot(int slotIndex)
        {
            for (int i = 0; i < hotbarSlots.Length; i++)
            {
                GameObject overlay = hotbarSlots[i].transform.parent.GetChild(0).gameObject;
                overlay.SetActive(false);
                if (i == slotIndex)
                {
                    overlay.SetActive(true);
                }
            }
        }

        bool CheckIndexExists(int index)
        {
            if (index >= 0 && index < hotbarSlots.Length)
            {
                return true;
            }
            throw new Exception("Invalid index!");
        }
    }
}

