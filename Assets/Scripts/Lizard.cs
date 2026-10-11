using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Lizard : MonoBehaviour
{
    public float speed = 2f;
    public LayerMask groundLayer;
    public float wallCheckDistance = 0.1f;
    public bool artFacesRight = true; // set false if your lizard sprite faces left

    Rigidbody2D rb;
    Collider2D col;
    SpriteRenderer sr;
    int dir = 1; // 1 = right, -1 = left

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        Bounds b = col.bounds;
        float frontX = dir > 0 ? b.max.x : b.min.x;

        // Ray 1: straight down, just past the front foot. No hit = ledge ahead.
        Vector2 footOrigin = new Vector2(frontX + 0.05f * dir, b.min.y + 0.05f);
        bool groundAhead = Physics2D.Raycast(footOrigin, Vector2.down, 0.3f, groundLayer);

        // Ray 2: forward from the middle of the body. Hit = wall ahead.
        Vector2 bodyOrigin = new Vector2(frontX, b.center.y);
        bool wallAhead = Physics2D.Raycast(bodyOrigin, Vector2.right * dir, wallCheckDistance, groundLayer);

        if (!groundAhead || wallAhead) dir = -dir;

        rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);

        if (sr != null) sr.flipX = artFacesRight ? dir < 0 : dir > 0;
    }

    void OnDrawGizmosSelected()
    {
        // Shows the two rays in the Scene view while the lizard is selected
        Collider2D c = GetComponent<Collider2D>();
        if (c == null) return;
        Bounds b = c.bounds;
        float frontX = dir > 0 ? b.max.x : b.min.x;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector2(frontX + 0.05f * dir, b.min.y + 0.05f), new Vector2(frontX + 0.05f * dir, b.min.y - 0.25f));
        Gizmos.DrawLine(new Vector2(frontX, b.center.y), new Vector2(frontX + wallCheckDistance * dir, b.center.y));
    }
}