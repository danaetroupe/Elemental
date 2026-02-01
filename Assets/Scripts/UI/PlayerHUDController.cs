using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    [SerializeField] private List<GameObject> imageDisplays;
    [SerializeField] private Color inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray color
    [SerializeField] private Color activeColor = Color.white;

    private List<Coroutine> activeAnimations = new List<Coroutine>();

    private void Start()
    {         
        DisableImages();
    }
    public void OnChange(List<GameObject> inventory, GameObject currentMask)
    {
        DisableImages();

        // Show inventory masks
        for (int i = 0; i < inventory.Count && i < imageDisplays.Count; i++)
        {
            GameObject display = imageDisplays[i];
            GameObject mask = inventory[i];

            display.SetActive(true);

            Image displayImage = display.GetComponent<Image>();
            if (displayImage == null) continue;

            // Set color based on whether it's the current mask
            bool isCurrentMask = mask == currentMask;
            displayImage.color = isCurrentMask ? activeColor : inactiveColor;

            // Get the TextureAnimator from the mask
            TextureAnimator animator = mask.GetComponent<TextureAnimator>();
            Texture2D[] textures = animator.GetTextures();
            if (animator != null && textures != null && textures.Length > 0)
            {
                // Start animating this display
                Coroutine animCoroutine = StartCoroutine(AnimateDisplay(displayImage, textures));
                activeAnimations.Add(animCoroutine);
            }
        }
    }

    private void DisableImages()
    {
        // Stop all running animations
        StopAllAnimations();

        if (imageDisplays == null) return;

        // Disable all displays first
        foreach (GameObject imgDisplay in imageDisplays)
        {
            imgDisplay.SetActive(false);
        }
    }
    private IEnumerator AnimateDisplay(Image displayImage, Texture2D[] textures)
    {
        if (textures == null || textures.Length == 0) yield break;

        int currentFrame = 0;
        float frameRate = 12f; // Adjust as needed
        float frameDelay = 1f / frameRate;

        while (true) // Loop forever until stopped
        {
            // Convert Texture2D to Sprite
            Texture2D currentTexture = textures[currentFrame];
            if (currentTexture != null)
            {
                Sprite sprite = Sprite.Create(
                    currentTexture,
                    new Rect(0, 0, currentTexture.width, currentTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
                displayImage.sprite = sprite;
            }

            currentFrame = (currentFrame + 1) % textures.Length;
            yield return new WaitForSeconds(frameDelay);
        }
    }

    private void StopAllAnimations()
    {
        foreach (Coroutine coroutine in activeAnimations)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        activeAnimations.Clear();
    }

    private void OnDisable()
    {
        StopAllAnimations();
    }
}