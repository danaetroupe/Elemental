using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private MeshRenderer bodyRenderer;
    [SerializeField] private MeshRenderer headRenderer;
    [SerializeField] private MeshRenderer leftArmRenderer;
    [SerializeField] private MeshRenderer rightArmRenderer;
    [SerializeField] private MeshRenderer leftFistRenderer;  // Detached fist
    [SerializeField] private MeshRenderer rightFistRenderer; // Detached fist

    [Header("Materials")]
    [SerializeField] private Material bodyNormal;
    [SerializeField] private Material bodyDazed;
    [SerializeField] private Material headNormal;
    [SerializeField] private Material headDazed;
    [SerializeField] private Material leftArmNeutral;
    [SerializeField] private Material rightArmNeutral;
    [SerializeField] private Material[] fistStompFrames; // 3 materials for stomp animation

    [Header("Combat Settings")]
    [SerializeField] private int enemiesToSpawn = 3;
    [SerializeField] private float dazedDuration = 5f;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Fight Trigger")]
    [SerializeField] private GameObject fightTrigger; // The trigger object to start the fight

    [Header("Animation Timing")]
    [SerializeField] private float headShakeDuration = 1f;
    [SerializeField] private float stompAnimationSpeed = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioClip roarSound;
    [SerializeField] private AudioClip defeatRoarSound; // Optional: different roar for defeat
    [SerializeField] private AudioSource audioSource;

    [Header("Health")]
    [SerializeField] private HealthController healthController;

    // State tracking
    private bool isFightActive = false;
    private bool fightStarted = false;
    private bool isDazed = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Vector3 headOriginalPosition;
    private float headShakeAmount = 0.1f;

    private void Start()
    {
        headOriginalPosition = headRenderer.transform.localPosition;

        // Subscribe to health events if needed
        if (healthController != null)
        {
            healthController.OnDeath.AddListener(OnBossDefeated);
        }

        // Setup trigger if it exists
        if (fightTrigger != null)
        {
            BossFightTrigger triggerComponent = fightTrigger.GetComponent<BossFightTrigger>();
            if (triggerComponent == null)
            {
                triggerComponent = fightTrigger.AddComponent<BossFightTrigger>();
            }
            triggerComponent.OnTriggerActivated += OnFightTriggered;
        }
        else
        {
            Debug.LogWarning("No fight trigger assigned! Boss fight will not start automatically.");
        }

        // Set initial materials
        SetInitialMaterials();
    }

    private void SetInitialMaterials()
    {
        if (bodyRenderer != null && bodyNormal != null)
            bodyRenderer.material = bodyNormal;

        if (headRenderer != null && headNormal != null)
            headRenderer.material = headNormal;

        if (leftArmRenderer != null && leftArmNeutral != null)
            leftArmRenderer.material = leftArmNeutral;

        if (rightArmRenderer != null && rightArmNeutral != null)
            rightArmRenderer.material = rightArmNeutral;

        // Hide fists initially
        if (leftFistRenderer != null)
            leftFistRenderer.enabled = false;

        if (rightFistRenderer != null)
            rightFistRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        if (healthController != null)
        {
            healthController.OnDeath.RemoveListener(OnBossDefeated);
        }

        if (fightTrigger != null)
        {
            BossFightTrigger triggerComponent = fightTrigger.GetComponent<BossFightTrigger>();
            if (triggerComponent != null)
            {
                triggerComponent.OnTriggerActivated -= OnFightTriggered;
            }
        }
    }

    private void OnFightTriggered()
    {
        if (!fightStarted)
        {
            fightStarted = true;
            StartBossFight();
        }
    }

    public void StartBossFight()
    {
        if (isFightActive) return;

        isFightActive = true;

        // Play roar to signal fight start
        if (audioSource != null && roarSound != null)
        {
            audioSource.PlayOneShot(roarSound);
        }

        // Disable the trigger so it can't be activated again
        if (fightTrigger != null)
        {
            fightTrigger.SetActive(false);
        }

        StartCoroutine(BossFightSequence());
    }

    private IEnumerator BossFightSequence()
    {
        // Wait for roar to finish (optional)
        if (roarSound != null)
        {
            yield return new WaitForSeconds(roarSound.length);
        }

        while (isFightActive)
        {
            // 1. Shake head angrily
            yield return StartCoroutine(ShakeHead());

            // 2. Stomp attack and spawn enemies
            yield return StartCoroutine(StompAttack());

            // 3. Wait for all enemies to die
            yield return StartCoroutine(WaitForEnemiesDeath());

            // 4. Enter dazed state
            yield return StartCoroutine(EnterDazedState());
        }
    }

    private IEnumerator ShakeHead()
    {
        float elapsed = 0f;

        while (elapsed < headShakeDuration)
        {
            float xOffset = Mathf.Sin(elapsed * 30f) * headShakeAmount;
            headRenderer.transform.localPosition = headOriginalPosition + new Vector3(xOffset, 0, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        headRenderer.transform.localPosition = headOriginalPosition;
    }

    private IEnumerator StompAttack()
    {
        // Hide neutral arms, show fists
        if (leftArmRenderer != null)
            leftArmRenderer.enabled = false;

        if (rightArmRenderer != null)
            rightArmRenderer.enabled = false;

        if (leftFistRenderer != null)
            leftFistRenderer.enabled = true;

        if (rightFistRenderer != null)
            rightFistRenderer.enabled = true;

        // Animate both fists stomping through material frames
        for (int i = 0; i < fistStompFrames.Length; i++)
        {
            if (fistStompFrames[i] != null)
            {
                if (leftFistRenderer != null)
                    leftFistRenderer.material = fistStompFrames[i];

                if (rightFistRenderer != null)
                    rightFistRenderer.material = fistStompFrames[i];
            }
            yield return new WaitForSeconds(stompAnimationSpeed);
        }

        // Spawn enemies on the final stomp frame
        SpawnEnemies();

        yield return new WaitForSeconds(stompAnimationSpeed);

        // Hide fists, show neutral arms again
        if (leftFistRenderer != null)
            leftFistRenderer.enabled = false;

        if (rightFistRenderer != null)
            rightFistRenderer.enabled = false;

        if (leftArmRenderer != null)
            leftArmRenderer.enabled = true;

        if (rightArmRenderer != null)
            rightArmRenderer.enabled = true;
    }

    private void SpawnEnemies()
    {
        spawnedEnemies.Clear();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }
    }

    private IEnumerator WaitForEnemiesDeath()
    {
        while (true)
        {
            // Remove null references (destroyed enemies)
            spawnedEnemies.RemoveAll(enemy => enemy == null);

            if (spawnedEnemies.Count == 0)
            {
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator EnterDazedState()
    {
        isDazed = true;

        // Swap to dazed materials
        if (bodyDazed != null)
            bodyRenderer.material = bodyDazed;

        if (headDazed != null)
            headRenderer.material = headDazed;

        // Hide arms and fists during dazed state
        leftArmRenderer.enabled = false;
        rightArmRenderer.enabled = false;

        if (leftFistRenderer != null)
            leftFistRenderer.enabled = false;

        if (rightFistRenderer != null)
            rightFistRenderer.enabled = false;

        yield return new WaitForSeconds(dazedDuration);

        // Return to normal materials
        if (bodyNormal != null)
            bodyRenderer.material = bodyNormal;

        if (headNormal != null)
            headRenderer.material = headNormal;

        // Show neutral arms again
        leftArmRenderer.enabled = true;
        rightArmRenderer.enabled = true;

        isDazed = false;
    }

    private void OnBossDefeated(Vector3 deathPosition)
    {
        isFightActive = false;
        StopAllCoroutines();

        // Play victory sequence
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator DefeatSequence()
    {
        // Play defeat roar sound (or reuse the same roar)
        AudioClip roarToPlay = defeatRoarSound != null ? defeatRoarSound : roarSound;

        if (audioSource != null && roarToPlay != null)
        {
            audioSource.PlayOneShot(roarToPlay);
        }

        // Swap to dazed materials for defeat
        if (bodyDazed != null)
            bodyRenderer.material = bodyDazed;

        if (headDazed != null)
            headRenderer.material = headDazed;

        // Hide all arms and fists
        leftArmRenderer.enabled = false;
        rightArmRenderer.enabled = false;

        if (leftFistRenderer != null)
            leftFistRenderer.enabled = false;

        if (rightFistRenderer != null)
            rightFistRenderer.enabled = false;

        yield return new WaitForSeconds(2f);

        // Trigger game win
        // GameManager.Instance?.OnPlayerWin(); // Uncomment if you have a GameManager
        Debug.Log("Player Wins!");
    }
}