using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Forest.UI
{
    public class StaminaBar : MonoBehaviour
    {
        [SerializeField] Slider slider;

        public float maxStamina = 100f;
        [HideInInspector] public float currentStamina;
        public float regenRate = 10f;
        public float regenDelay = 1f;
        [HideInInspector] public float drainRate;

        bool isDraining;
        bool isRegenerating;

        void Start()
        {
            slider.value = maxStamina;
        }

        void Update()
        {
            currentStamina = slider.value;

            if (isDraining && slider.value > 0f)
            {
                DrainStamina(drainRate);
            }
        }

        public void StartDrainingStamina(float newDrainRate)
        {
            if (!isDraining)
            {
                drainRate = newDrainRate;
                isRegenerating = false;
                isDraining = true;
            }
        }

        public void StopDrainingStamina()
        {
            if (isDraining)
            {
                isDraining = false;
                StopAllCoroutines();
                StartCoroutine(RegenerateStaminaAfterDelay());
            }
        }

        void DrainStamina(float drainRate)
        {
            slider.value -= drainRate * Time.deltaTime;
            slider.value = Mathf.Max(slider.value, 0); // Ensure stamina doesn't go below 0
        }

        IEnumerator RegenerateStaminaAfterDelay()
        {
            isRegenerating = true;

            yield return new WaitForSeconds(regenDelay);

            while (slider.value < maxStamina && !isDraining)
            {
                slider.value += regenRate * Time.deltaTime;
                slider.value = Mathf.Min(slider.value, maxStamina); // Ensure stamina doesn't exceed max
                yield return null;
            }

            isRegenerating = false;
        }

        public void RemoveStamina(float amount)
        {
            if (amount <= 0) return;

            slider.value -= amount;
            slider.value = Mathf.Max(slider.value, 0); // Ensure stamina doesn't go below 0

            StopAllCoroutines();
            StartCoroutine(RegenerateStaminaAfterDelay());
        }
    }
}