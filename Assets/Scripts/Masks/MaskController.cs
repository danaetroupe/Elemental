using UnityEngine;

public class MaskController : MonoBehaviour
{
    [Header("Character Transformation")]
    [SerializeField] private Color characterColor = Color.white;

    [Header("Power")]
    [SerializeField] private Power power;

    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 initialPosition;
    private bool isSpawned = false;

    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (!isSpawned) return;

        // Visual effects
        transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        float newY = initialPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void HideSprite()
    {
        if (gameObject.TryGetComponent<MeshRenderer>(out MeshRenderer meshRenderer))
        {
            meshRenderer.enabled = false;
        }

        isSpawned = false;
    }

    public void OnPickup(PlayerControls player)
    {
        if (player != null)
        {
            player.EquipMask(gameObject);
        }
    }

    public Power GetPower()
    {
        return power;
    }

    public void SetPower(Power newPower)
    {
        power = newPower;
        Debug.Log($"Power set to: {(newPower != null ? newPower.GetType().Name : "None")}");
    }

    public void UsePower(Vector3 aimTarget = default)
    {
        power.UsePower(aimTarget);
    }
}