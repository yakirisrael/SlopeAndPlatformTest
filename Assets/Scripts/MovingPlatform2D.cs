using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform2D : MonoBehaviour
{
    [Header("Movement")]
    public Vector2 moveDirection = Vector2.right; // direction of movement
    public float moveDistance = 3f;              // how far it moves from start
    public float speed = 2f;                     // platform speed

    Rigidbody2D rb;
    Vector2 startPos;
    Vector2 lastPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        startPos = rb.position;
        lastPos = rb.position;
    }

    void FixedUpdate()
    {
        // determine platform destination
        Vector2 targetPos = startPos + moveDirection.normalized * moveDistance;
        
        float t = Mathf.PingPong(Time.time * speed, 1f); // ping pong between 0 to 1  and vice versa over time
        Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);

        rb.MovePosition(newPos);

        // calculate platform's velocity
        Velocity = (newPos - lastPos) / Time.fixedDeltaTime;
        
        lastPos = newPos;
    }

    public Vector2 Velocity { get; private set; }
    
}