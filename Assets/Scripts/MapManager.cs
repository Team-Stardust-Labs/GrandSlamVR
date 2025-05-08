using UnityEngine;

public class MapManager : MonoBehaviour
{

    [SerializeField] private GameObject LocalPlayer;
    [SerializeField] private Transform SpawnPlayer1;
    [SerializeField] private Transform SpawnPlayer2;

    public void joinTeam1() {
        LocalPlayer.transform.position = SpawnPlayer1.position;
        LocalPlayer.transform.rotation = SpawnPlayer1.rotation;
    }

    public void joinTeam2() {
        LocalPlayer.transform.position = SpawnPlayer2.position;
        LocalPlayer.transform.rotation = SpawnPlayer2.rotation;
    }
}
