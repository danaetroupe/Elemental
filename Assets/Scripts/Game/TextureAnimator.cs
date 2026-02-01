using UnityEngine;

public class TextureAnimator : MonoBehaviour
{
    [SerializeField] Texture2D[] textures; 
    [SerializeField] float frameRate = 12f;

    private Renderer rend;
    private int currentFrame = 0;
    private float timer = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 1f / frameRate)
        {
            currentFrame = (currentFrame + 1) % textures.Length;
            rend.material.mainTexture = textures[currentFrame];
            timer = 0f;
        }
    }
}