using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Forest.UI;
using Unity.VisualScripting;

namespace Forest.Inventory
{
    public class HotbarManager : MonoBehaviour
    {
        public static HotbarManager Instance { get; private set; }

        [SerializeField] float flashDuration;

        [SerializeField] HotbarSlot[] hotbarSlots;
        public HotbarSlot[] HotbarSlots => hotbarSlots;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            HighlightHotbarSlot(0); // TODO: Save highlighted slot
        }

        public void SetSlotSprite(int slotIndex, Sprite itemSprite)
        {   
            if (!CheckIndexExists(slotIndex)) { return; }

            Image slotImage = hotbarSlots[slotIndex].slotImage;
            slotImage.sprite = itemSprite;
            slotImage.enabled = itemSprite != null;
            slotImage.SetNativeSize();
        }

        public void HighlightHotbarSlot(int slotIndex)
        {
            EndFlash();
            hotbarSlots[slotIndex].Active = true;
        }

        public void FlashRed(int activeSlot)
        {
            StopAllCoroutines();
            StartCoroutine(Flash(activeSlot));
        }

        void EndFlash()
        {
            StopAllCoroutines();
            foreach (HotbarSlot slot in hotbarSlots)
            {
                slot.overlayImage.enabled = false;
                slot.overlayImage.color = Color.white;
            }
        }

        IEnumerator Flash(int activeSlot)
        {
            foreach (HotbarSlot slot in hotbarSlots)
            {
                slot.overlayImage.enabled = true;
                slot.overlayImage.color = Color.red;
            }

            yield return new WaitForSeconds(flashDuration);
            
            foreach (HotbarSlot slot in hotbarSlots)
            {
                slot.overlayImage.enabled = false;
                slot.overlayImage.color = Color.white;
            }
            hotbarSlots[activeSlot].Active = true;
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

