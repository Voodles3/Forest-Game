using System.Collections;
using System.Collections.Generic;
using Forest.Inventory;
using UnityEngine;
public class HoverEffect : MonoBehaviour
{
    public Color glowColor = Color.white;
    [Range(0f, 0.1f)] public float glowIntensity = 0.05f;

    Renderer[] renderers;
    Color originalEmissionColor;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0 && renderers[0].material.HasProperty("_EmissionColor"))
        {
            originalEmissionColor = renderers[0].material.GetColor("_EmissionColor");
        }
    }

    void OnMouseEnter()
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

    void OnMouseExit()
    {
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
