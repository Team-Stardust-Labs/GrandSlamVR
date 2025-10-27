using UnityEngine;

public class TeamColorChanger : MonoBehaviour
{

    public GameObject controllerLeft;
    public GameObject controllerRight;
    public GameObject playerModelPrefab;

    public Material blueMaterial;
    public Material redMaterial;


    void Start()
    {
        controllerLeft.GetComponent<Renderer>().material.SetFloat("_MatcapIntensity", 0.45f);
        controllerRight.GetComponent<Renderer>().material.SetFloat("_MatcapIntensity", 0.45f);

        if (AssignPlayerColor.isBlue())
        {
            // Host
            // Set the color of the material to blue
            controllerLeft.GetComponent<Renderer>().material.SetColor("_LightColor_02", blueMaterial.GetColor("_Color"));
            controllerRight.GetComponent<Renderer>().material.SetColor("_LightColor_02", blueMaterial.GetColor("_Color"));

            // Set color of all child renderers of the player model
            /*GameObject playerModel = Instantiate(playerModelPrefab);
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material = redMaterial; // Inverse team colors
            }*/
        }
        else
        {
            // Client
            // Set the color of the material to red
            controllerLeft.GetComponent<Renderer>().material.SetColor("_LightColor_02", redMaterial.GetColor("_Color"));
            controllerRight.GetComponent<Renderer>().material.SetColor("_LightColor_02", redMaterial.GetColor("_Color"));

            // Set color of all child renderers of the player model
            /*GameObject playerModel = Instantiate(playerModelPrefab);
            Renderer[] renderers = playerModel.GetComponentsInChildren<Renderer>();
            foreach (Renderer rend in renderers)
            {
                rend.material = blueMaterial; // Inverse team colors
            }*/
        }
    }
}
