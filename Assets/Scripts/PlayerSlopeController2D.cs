using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Slope")]
    public float stickToGroundForce = 20f;
    public float maxSlopeAngle = 60f;

    Rigidbody2D rb;
    
    bool isGrounded;
    bool jumpedThisFrame;
    
    Vector2 slopeNormal = Vector2.up;
    
    MovingPlatform2D groundPlatform;

    float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            Jump();
    }

    void FixedUpdate()
    {
        CheckGround();

        
        // if press jump button - ignore movement for first frame, otherwise it will stick to the slope
        if (jumpedThisFrame)
        {
            jumpedThisFrame = false;
            return;
        }

        // handle movement, including sticking to slope
        HandleMovement();
    }

    // ---------------- GROUND CHECK ----------------

    void CheckGround()
    {
        Collider2D col = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // check overlap with circle which is place on the legs of the player
        // if it's overlap with ground layer - iGrounded = true
        if (col)
        {
            isGrounded = true;
            groundPlatform = col.GetComponent<MovingPlatform2D>();

            // if on ground - make raycast to it to find the normal of the ground
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.6f, groundLayer);

            if (hit)
                slopeNormal = hit.normal; 
        }
        else
        {
            isGrounded = false;
            groundPlatform = null;
            slopeNormal = Vector2.up;
        }
    }

    // ---------------- MOVEMENT ----------------

    void HandleMovement()
    {
        Vector2 platformDelta = Vector2.zero;
        
        // if player on moving platform - use the velocity of the platform as offset for the player's velocity 
        if (groundPlatform != null)
            platformDelta = groundPlatform.Velocity * Time.fixedDeltaTime;

        if (isGrounded)
        {
            //move according to input on x + velocity offset from platform
            float x = moveInput * moveSpeed + platformDelta.x;
            // maintain the velocity on y (because the player is on the platform and moves with it already)
            float y = rb.linearVelocity.y;

            rb.linearVelocity = new Vector2(x, y);

            // Stick player slightly to slope by adding force every frame - in opposite direction of slope normal (it pushes against the slope)
            rb.AddForce(-slopeNormal * stickToGroundForce, ForceMode2D.Force);
        }
    }

    // ---------------- JUMP ----------------

    void Jump()
    {
        // prevent jumping if is in the air
        if (!isGrounded)
            return;

        jumpedThisFrame = true;
        isGrounded = false;
        groundPlatform = null;

        // maintain the velocity on x when jumping
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        
        // give impulse upward
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ---------------- DEBUG ----------------

    // gizmos will be shown in scene tab, if select the player
    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;

        // will show sphere collider that detects the collision of player's feet 
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // will show the normal from the ground
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + (Vector3)slopeNormal);
    }
}
