using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum DOOR_DIRECTION
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class Transporter : MonoBehaviour
{
    [SerializeField] public DOOR_DIRECTION enterFromDirection;
    [SerializeField] public DOOR_DIRECTION exitToDirection;
    [SerializeField] private string targetLevelName;

    [Header("Object References")]
    [SerializeField] Transform spawnPoint;

    private string TRANSPORT_DIRECTION = "TransportDirection";

    private void Start()
    {
        int lastExitDirection = PlayerPrefs.GetInt(TRANSPORT_DIRECTION);
        if ((DOOR_DIRECTION)lastExitDirection == enterFromDirection)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = spawnPoint.position;
            }
        }
    }

    public void ExitScene()
    {
        // Store which door the player used
        PlayerPrefs.SetInt(TRANSPORT_DIRECTION, (int)exitToDirection);
        SceneManager.LoadScene(targetLevelName);
    }
}
