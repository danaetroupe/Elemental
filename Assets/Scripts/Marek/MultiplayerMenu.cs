using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool isEnabled = true;

    public void ToggleSelected()
    {
        if (target == null) return;

        target.SetActive(!isEnabled);
        isEnabled = !isEnabled;
    }

}
