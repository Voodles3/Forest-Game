using UnityEngine;

namespace Forest.Interaction
{
    public class HoverEffect : MonoBehaviour
    {
        public Color glowColor = Color.white;
        [Range(0f, 0.2f)] public float glowIntensity = 0.05f;

        Renderer[] renderers;
        Color originalEmissionColor;
        bool hovering = true;

        void Awake()
        {
            if (TryGetComponent(out Renderer renderer))
            {
                renderers = new Renderer[] {renderer};
            }
            else
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            if (renderers.Length > 0 && renderers[0].material.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = renderers[0].material.GetColor("_EmissionColor");
            }
        }

        void Update()
        {
            if (InteractionController.Instance.CurrentHoveredObject == gameObject)
            {
                if (!hovering)
                {
                    hovering = true;
                    StartHoverEffect();
                }
            }
            else
            {
                if (hovering)
                {
                    StopHoverEffect();
                }
            }
        }

        void StartHoverEffect()
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);

                    renderer.material.EnableKeyword("_EMISSION");
                }
            }
        }

        public void StopHoverEffect()
        {
            hovering = false;
            foreach (Renderer renderer in renderers)
            {
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.SetColor("_EmissionColor", originalEmissionColor);
                    renderer.material.DisableKeyword("_EMISSION");
                }
            }
        }
    }
}

