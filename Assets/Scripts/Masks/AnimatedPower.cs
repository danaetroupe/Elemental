using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnimatedPower : Power
{
    [SerializeField] List<Sprite> frames;
    [SerializeField] float frameRate = 12f;

    private Canvas mainCanvas;

    void Start()
    {
        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        if (!mainCanvas)
        {
            Debug.LogError("MainCanvas not found in scene. Please add a Canvas with the tag 'MainCanvas'.");
        }
    }
    protected override void DoBehavior()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();

        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPoint
        );

        GameObject imageObject = new GameObject("ImageProjectile");
        Image image = imageObject.AddComponent<Image>();
        image.sprite = frames[0];

        imageObject.transform.SetParent(mainCanvas.transform, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = localPoint;

        StartCoroutine(AnimateFrames(imageObject, image));

        // Spawn invisible "projectile"
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 worldPosition = hit.point;
            SpawnProjectile(worldPosition, Vector3.down);
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
}
