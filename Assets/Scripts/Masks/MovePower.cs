using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovePower : Power
{
    [SerializeField] List<Sprite> frames;
    [SerializeField] float frameRate = 12f;
    [SerializeField] Vector2 animationOffset = new Vector2(0, 0f);
    [SerializeField] float pushForce = 10f;
    [SerializeField] float pushRadius = 5f;

    private Canvas mainCanvas;
    void Start()
    {
        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        if (!mainCanvas)
        {
            Debug.LogError("MainCanvas not found in scene. Please add a Canvas with the tag 'MainCanvas'.");
        }
    }

    protected override void DoBehavior(Vector3 aimTarget)
    {
        // UI animation uses local mouse for visuals (client-side only)
        if (mainCanvas != null && Mouse.current != null)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                out Vector2 localPoint
            );

            // Create and setup UI animation
            GameObject imageObject = new GameObject("ImageProjectile");
            Image image = imageObject.AddComponent<Image>();
            image.sprite = frames[0];
            imageObject.transform.SetParent(mainCanvas.transform, false);
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = localPoint + animationOffset;
            rectTransform.localScale = Vector3.one * 3f;

            StartCoroutine(AnimateFrames(imageObject, image));
        }

        // Push enemies away from aimTarget (passed from client via ServerRpc)
        PushEnemiesAway(aimTarget);

        // Spawn the projectile at aimTarget
        SpawnProjectile(aimTarget + Vector3.up, Vector3.down);
    }

    void PushEnemiesAway(Vector3 center)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, pushRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            // Check if the collider has the "Enemy" tag
            if (hitCollider.CompareTag("Enemy"))
            {
                // Calculate direction from center to enemy
                Vector3 pushDirection = (hitCollider.transform.position - center).normalized;

                // Try to apply force via Rigidbody
                if (hitCollider.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                }
                // Alternative: directly move the transform if no Rigidbody
                else
                {
                    hitCollider.transform.position += pushDirection * pushForce * Time.deltaTime;
                }
            }
        }
    }

    IEnumerator AnimateFrames(GameObject imageObject, Image image)
    {
        float frameDelay = 1f / frameRate;
        foreach (Sprite frame in frames)
        {
            image.sprite = frame;
            yield return new WaitForSeconds(frameDelay);
        }
        Destroy(imageObject);
    }

    // Optional: Visualize the push radius in the editor
    void OnDrawGizmosSelected()
    {
        if (Mouse.current != null)
        {
            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(hit.point, pushRadius);
            }
        }
    }
}