using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bee : MonoBehaviour
{
    public float patrolDistance = 3f; // how far it flies each way from its start point
    public float speed = 2f;
    public float bobHeight = 0.4f;
    public float bobSpeed = 3f;
    public bool vertical = false;     // tick to fly up and down instead of side to side
    public bool artFacesRight = true;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 startPos;
    float t;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        startPos = rb.position;
        t = Random.Range(0f, 10f); // so bees placed together don't move in sync
    }

    void FixedUpdate()
    {
        t += Time.fixedDeltaTime;
        float offset = Mathf.PingPong(t * speed, patrolDistance * 2f) - patrolDistance;
        float bob = Mathf.Sin(t * bobSpeed) * bobHeight;

        Vector2 target = vertical
            ? startPos + new Vector2(bob, offset)
            : startPos + new Vector2(offset, bob);

        if (sr != null && !vertical)
        {
            bool movingLeft = target.x < rb.position.x;
            sr.flipX = artFacesRight ? movingLeft : !movingLeft;
        }

        rb.MovePosition(target);
    }

    void OnDrawGizmosSelected()
    {
        // Shows the flight path in the Scene view while the bee is selected
        Vector2 center = Application.isPlaying ? startPos : (Vector2)transform.position;
        Vector2 axis = vertical ? Vector2.up : Vector2.right;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center - axis * patrolDistance, center + axis * patrolDistance);
    }
}