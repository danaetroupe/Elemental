using System.Collections;
using Unity.Netcode;
using Unity.Mathematics;
using UnityEngine;

public class EnemyController : RadialPower
{
    [Header("Movement Settings")]
    [SerializeField] private bool3 axis;
    [SerializeField] private float range = 5f; 
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private float timeBetweenFire = 2f;
    [Tooltip("Only fires while a player is within this distance.")]
    [SerializeField] private float aggroRange = 25f;

    [SerializeField] private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    private bool movingToTarget = true;
    private Coroutine fireRoutine;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        lastPosition = transform.position;
        CalculateTargetPosition();

       StartCoroutine(BeginFiringWhenNetworkReady());
    }

    void Update()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening &&
            !NetworkManager.Singleton.IsServer)
        {
            AnimateFromMovement();
            return;
        }

        MoveEnemy();
    }

    private void AnimateFromMovement()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;
        if (delta.sqrMagnitude < 0.000001f) return;

        UpdateWalkAnimation(delta.normalized);
    }

    private void UpdateWalkAnimation(Vector3 direction)
    {
        bool horizontal = Mathf.Abs(direction.x) >= Mathf.Abs(direction.z);

        if (animator != null)
        {
            animator.SetBool("WalkRight", horizontal);
            animator.SetBool("WalkUp", !horizontal);
        }

        if (horizontal && spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    private bool IsPlayerInRange()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if ((player.transform.position - transform.position).sqrMagnitude <= aggroRange * aggroRange)
                return true;
        }
        return false;
    }

    private IEnumerator BeginFiringWhenNetworkReady()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            fireRoutine = StartCoroutine(FireProjectiles());
            yield break;
        }

        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening);

        if (!NetworkManager.Singleton.IsServer)
        {
            yield break;
        }

        fireRoutine = StartCoroutine(FireProjectiles());
    }

    private void CalculateTargetPosition()
    {
        targetPosition = startPosition;

        if (axis.x) targetPosition.x += range;
        if (axis.y) targetPosition.y += range;
        if (axis.z) targetPosition.z += range;
    }

    private void MoveEnemy()
    {
        Vector3 destination = movingToTarget ? targetPosition : startPosition;
        Vector3 direction = (destination - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        UpdateWalkAnimation(direction);

        if (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            movingToTarget = !movingToTarget;
        }
    }

    private IEnumerator FireProjectiles()
    {
        while(true)
        {
            if (IsPlayerInRange())
            {
                UsePower();
            }
            yield return new WaitForSeconds(timeBetweenFire);
        }
    }

    /// <summary>
    /// Override to broadcast enemy attack sounds to all clients via HealthController.
    /// </summary>
    protected override void PlayAttackSound()
    {
        if (attackSound == null) return;

        // Use HealthController (which is a NetworkBehaviour) to broadcast sound
        var healthController = GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.PlayPowerSoundNetworked(attackSoundVolume);
        }
        else
        {
            // Fallback if no HealthController
            AudioSource.PlayClipAtPoint(attackSound, transform.position, attackSoundVolume);
        }
    }
}