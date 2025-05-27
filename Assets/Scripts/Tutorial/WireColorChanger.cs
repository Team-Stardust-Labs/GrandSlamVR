using UnityEngine;

public class WireColorChanger : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color enterColor;


    public void ChangeWireColor()
    {
        if (targetRenderer != null && targetRenderer.material.HasProperty("_WireColor"))
        {
            targetRenderer.material.SetColor("_WireColor", enterColor);
        }
    }

    public void ResetWireColor()
    {
        if (targetRenderer != null && targetRenderer.material.HasProperty("_WireColor"))
        {
            targetRenderer.material.SetColor("_WireColor", normalColor);
        }
    }
}
