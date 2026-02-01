using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUIController : MonoBehaviour
{
    [Header("Health Bar Images")]
    [SerializeField] private Image outerImage; // Background/border of health bar
    [SerializeField] private Image innerImage; // The fill that changes based on health

    [Header("Color Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f; // 30% health triggers color change

    [Header("Animation Settings")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;

    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;

    void Start()
    {
        if (innerImage != null)
        {
            innerImage.fillAmount = 1f;
            innerImage.color = healthyColor;
            innerImage.type = Image.Type.Filled;
            innerImage.fillMethod = Image.FillMethod.Horizontal;
        }
    }

    void Update()
    {
        if (smoothTransition && innerImage != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            innerImage.fillAmount = currentFillAmount;
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (innerImage == null) return;

        float healthPercent = (float)currentHealth / maxHealth;
        healthPercent = Mathf.Clamp01(healthPercent);

        targetFillAmount = healthPercent;

        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
            innerImage.fillAmount = currentFillAmount;
        }

        // Change color based on health percentage
        if (healthPercent <= lowHealthThreshold)
        {
            innerImage.color = lowHealthColor;
        }
        else
        {
            innerImage.color = healthyColor;
        }
    }
    public void UpdateHealthBarPercent(float healthPercent)
    {
        if (innerImage == null) return;

        healthPercent = Mathf.Clamp01(healthPercent);
        targetFillAmount = healthPercent;

        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
            innerImage.fillAmount = currentFillAmount;
        }

        if (healthPercent <= lowHealthThreshold)
        {
            innerImage.color = lowHealthColor;
        }
        else
        {
            innerImage.color = healthyColor;
        }
    }

    public void ResetHealthBar()
    {
        targetFillAmount = 1f;
        currentFillAmount = 1f;
        if (innerImage != null)
        {
            innerImage.fillAmount = 1f;
            innerImage.color = healthyColor;
        }
    }
}