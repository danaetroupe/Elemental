using UnityEngine;

public class MaskController : MonoBehaviour
{
    [Header("Mask Visual")]
    [SerializeField] private Sprite maskSprite;

    [Header("Character Transformation")]
    [SerializeField] private Sprite characterSprite;
    [SerializeField] private Color characterColor = Color.white;

    [Header("Power")]
    [SerializeField] private Power power;

    [Header("Components")]
    private SpriteRenderer spriteRenderer;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 initialPosition;
    private bool isSpawned = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (maskSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = maskSprite;
        }
    }

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

    public void Spawn(Vector3 location)
    {
        transform.position = location;
        initialPosition = location;
        isSpawned = true;

        // Make sure the sprite is visible
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            if (maskSprite != null)
            {
                spriteRenderer.sprite = maskSprite;
            }
        }

        // Add collider for pickup if not present
        if (GetComponent<Collider2D>() == null && GetComponent<Collider>() == null)
        {
            // For 3D project, add a sphere collider
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = pickupRange;
        }

        Debug.Log($"Mask spawned at: {location}");
    }
    public void DestroySprite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        isSpawned = false;
    }

    public void OnPickup(PlayerControls player)
    {
        if (player != null)
        {
            player.EquipMask(gameObject);
            DestroySprite();
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
}