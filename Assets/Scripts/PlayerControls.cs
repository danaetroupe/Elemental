using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator mAnimator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //mAnimator = GetComponent<Animator>();
    }

    public void SetMovement()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;
    }

    void HandleAnimations()
    {
        if (mAnimator)
        {
            mAnimator.SetBool("isBack", Input.GetKey(KeyCode.W));
            mAnimator.SetBool("isRight", Input.GetKey(KeyCode.D));
            mAnimator.SetBool("isForward", Input.GetKey(KeyCode.S));
            mAnimator.SetBool("isLeft", Input.GetKey(KeyCode.A));
        }
    }

    void FixedUpdate()
    {
        SetMovement();
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
