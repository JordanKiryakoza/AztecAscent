using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 60f;
    public float deceleration = 70f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;        // grace period after walking off a ledge
    public float jumpBufferTime = 0.1f;    // jump pressed just before landing still counts
    public float jumpCutMultiplier = 0.5f; // releasing jump early gives a shorter hop
    public float fallGravityMultiplier = 1.6f;
    public float maxFallSpeed = 20f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Knockback")]
    public Vector2 knockbackForce = new Vector2(8f, 6f);
    public float stunTime = 0.3f;
    public float invulnerableTime = 1f;

    [Header("Visuals and Polish (optional, Phases 6 and 9)")]
    public Transform visual;
    public ParticleSystem landDust;
    public CinemachineImpulseSource impulse;
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip hitSound;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;
    float baseGravity;
    float coyoteCounter;
    float jumpBufferCounter;
    float inputX;
    bool isGrounded;
    bool wasGrounded;
    bool isStunned;
    bool isInvulnerable;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        anim = GetComponentInChildren<Animator>();
        baseGravity = rb.gravityScale;
    }

    void Update()
    {
        // ---- Read input (keyboard and gamepad) ----
        inputX = 0f;
        bool jumpPressed = false;
        bool jumpReleased = false;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) inputX -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) inputX += 1f;
            jumpPressed |= kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            jumpReleased |= kb.spaceKey.wasReleasedThisFrame || kb.wKey.wasReleasedThisFrame || kb.upArrowKey.wasReleasedThisFrame;
        }

        var pad = Gamepad.current;
        if (pad != null)
        {
            float stick = pad.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > 0.3f) inputX = Mathf.Sign(stick);
            jumpPressed |= pad.buttonSouth.wasPressedThisFrame;
            jumpReleased |= pad.buttonSouth.wasReleasedThisFrame;
        }

        if (isStunned)
        {
            inputX = 0f;
            jumpPressed = false;
            jumpReleased = false;
        }

        // ---- Ground check, coyote time, jump buffer ----
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded && !wasGrounded) OnLanded();

        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;
        jumpBufferCounter = jumpPressed ? jumpBufferTime : jumpBufferCounter - Time.deltaTime;

        // ---- Jump ----
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            if (visual != null) visual.localScale = new Vector3(0.8f, 1.2f, 1f); // stretch
            PlaySound(jumpSound);
        }

        // ---- Variable jump height: releasing early cuts the jump ----
        if (jumpReleased && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);

        // ---- Fall faster than you rise ----
        rb.gravityScale = rb.linearVelocity.y < 0f ? baseGravity * fallGravityMultiplier : baseGravity;

        // ---- Face the way you're moving ----
        if (sr != null && inputX != 0f) sr.flipX = inputX < 0f;

        // ---- Ease squash/stretch back to normal ----
        if (visual != null)
            visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one, 12f * Time.deltaTime);

        // ---- Feed the Animator (only once Phase 6 adds one) ----
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
            anim.SetBool("IsGrounded", isGrounded);
            anim.SetFloat("VelocityY", rb.linearVelocity.y);
        }
    }

    void FixedUpdate()
    {
        // Cap fall speed so long drops stay controllable
        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);

        // While stunned, let the knockback carry the player
        if (isStunned) return;

        float targetSpeed = inputX * moveSpeed;
        float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
    }

    void OnLanded()
    {
        if (visual != null) visual.localScale = new Vector3(1.2f, 0.8f, 1f); // squash
        if (landDust != null) landDust.Play();
        PlaySound(landSound);
    }

    // ---- Getting hit by a lizard or bee ----
    void OnCollisionEnter2D(Collision2D other) { CheckEnemy(other.gameObject); }
    void OnCollisionStay2D(Collision2D other) { CheckEnemy(other.gameObject); }

    void CheckEnemy(GameObject other)
    {
        if (other.CompareTag("Enemy")) Knockback(other.transform.position);
    }

    public void Knockback(Vector2 fromPosition)
    {
        if (isInvulnerable) return;
        float dir = transform.position.x >= fromPosition.x ? 1f : -1f; // push away from the mob
        rb.linearVelocity = new Vector2(dir * knockbackForce.x, knockbackForce.y);
        if (impulse != null) impulse.GenerateImpulse();
        PlaySound(hitSound);
        StartCoroutine(StunRoutine());
        StartCoroutine(InvulnerableRoutine());
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }

    IEnumerator InvulnerableRoutine()
    {
        isInvulnerable = true;
        float t = 0f;
        while (t < invulnerableTime)
        {
            if (sr != null) sr.enabled = !sr.enabled; // blink
            yield return new WaitForSeconds(0.1f);
            t += 0.1f;
        }
        if (sr != null) sr.enabled = true;
        isInvulnerable = false;
    }

    // ---- Used by the GameManager for checkpoints ----
    public void TeleportTo(Vector3 position)
    {
        rb.linearVelocity = Vector2.zero;
        rb.position = position;
        transform.position = position;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}