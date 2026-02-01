using UnityEngine;

public class MaskSelection : MonoBehaviour
{
    [SerializeField] private GameObject mask;
    
    public void SetMask()
    {
        // TODO: Make this work for multiplayer
        GameObject player = GameObject.FindWithTag("Player");
        GameObject newMask = Instantiate(mask);
        player.GetComponent<PlayerControls>().EquipMask(newMask);
    }
}
