using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// NOTE: The movement for this script uses the new InputSystem. The player needs to have a PlayerInput
// component added and the Behaviour should be set to Send Messages so that the OnMove and OnFire methods
// actually trigger

public class Player : MonoBehaviour
{
    public float collisionOffset = 0.05f;
    public ContactFilter2D movementFilter;

    private Vector2 moveInput;
    private List<RaycastHit2D> castCollisions = new List<RaycastHit2D>();
    private Rigidbody2D rb;
    [HideInInspector]
    public SpriteRenderer lightSpriteRenderer;

    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lightSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    public void FixedUpdate()
    {
        if (!GameSystem.Instance.GetCurrentLevel().running)
        {
            moveInput = Vector2.zero;
            return;
        }

        // Try to move player in input direction, followed by left right and up down input if failed
        bool success = MovePlayer(moveInput);

        if (!success)
        {
            // Try Left / Right
            success = MovePlayer(new Vector2(moveInput.x, 0));

            if (!success)
            {
                success = MovePlayer(new Vector2(0, moveInput.y));
            }
        }

    }

    // Tries to move the player in a direction by casting in that direction by the amount
    // moved plus an offset. If no collisions are found, it moves the players
    // Returns true or false depending on if a move was executed
    public bool MovePlayer(Vector2 direction)
    {
        var game = GameSystem.Instance;

        // Check for potential collisions
        int count = rb.Cast(
            direction, // X and Y values between -1 and 1 that represent the direction from the body to look for collisions
            movementFilter, // The settings that determine where a collision can occur on such as layers to collide with
            castCollisions, // List of collisions to store the found collisions into after the Cast is finished
            game.playerSpeed * Time.fixedDeltaTime + collisionOffset // The amount to cast equal to the movement plus an offset
        );

        if (count == 0)
        {
            Vector2 moveVector = direction * game.playerSpeed * Time.fixedDeltaTime;

            // No collisions
            rb.MovePosition(rb.position + moveVector);
            return true;
        }
        else
        {
            // Print collisions
            // foreach (RaycastHit2D hit in castCollisions)
            // {
            //     Debug.Log(hit.collider.gameObject.name);
            // }

            return false;
        }
    }

    public void OnMove(InputValue value)
    {
        var game = GameSystem.Instance;
        var level = game.GetCurrentLevel();

        var movement = value.Get<Vector2>();
        if (!level.running)
        {
            lightSpriteRenderer.color = movement == Vector2.zero ? level.playerLight : Color.red;
            return;
        };
        moveInput = movement;
        level.AddPathPoint(transform.position);
    }
}