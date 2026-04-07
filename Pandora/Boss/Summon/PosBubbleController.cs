using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PosBubbleController : MonoBehaviour
{
    [Header("移動パラメータ")]
    [SerializeField] public float speed = 10f;      // 上昇速度
    [SerializeField] public float height = 2.5f;    // 上昇高
    [SerializeField] public float gravityScale = 2f; // 落下時の重力

    [Header("爆発パラメータ")]
    [SerializeField] public float explosionDuration = 2f; // 爆発フェーズの長さ
    [SerializeField] public float blinkDuration = 0.1f;   // 点滅フェーズの長さ
    [SerializeField] private LayerMask groundLayers;

    [Header("ヒットボックス")]
    [SerializeField] private GameObject hitbox;

    private enum Phase
    {
        Rising,     // 上昇
        Falling,    // 落下
        Blinking,   // 点滅
        Exploding   // 爆発
    }

    private Phase phase;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider2D;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float startY;             // 生成時のY座標
    private float timer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        circleCollider2D.enabled = false;
        hitbox.SetActive(false);
        animator.enabled = false;

        circleCollider2D.includeLayers = 0;
        circleCollider2D.excludeLayers = ~groundLayers.value;

        startY = transform.position.y;
        phase = Phase.Rising;
        rb.gravityScale = 0f; // 上昇中は重力を切る
    }

    void Update()
    {
        switch (phase)
        {
            case Phase.Rising:
                UpdateRising();
                break;

            case Phase.Falling:
                break;

            case Phase.Blinking:
                UpadateBlinking();
                break;

            case Phase.Exploding:
                UpdateExploding();
                break;
        }
    }

    private void UpdateRising()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, speed);

        if (transform.position.y >= startY + height)
        {
            phase = Phase.Falling;
            rb.gravityScale = gravityScale;
            circleCollider2D.enabled = true;
        }
    }

    private void UpadateBlinking()
    {
        // 点滅処理
        timer += Time.deltaTime;
        //spriteRenderer.enabled = !spriteRenderer.enabled;

        if (timer >= blinkDuration)
        {
            timer = 0f;
            //spriteRenderer.enabled = !spriteRenderer.enabled;
            phase = Phase.Exploding;
            hitbox.SetActive(true);
        }
    }

    private void UpdateExploding()
    {
        timer += Time.deltaTime;

        if (timer >= explosionDuration)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & groundLayers.value) == 0) return;
        //if (!IsHitGroundFromAbove(other)) return;

        phase = Phase.Blinking;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        timer = 0f;

        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("Default", 0, 0f);
        }
    }

    private bool IsHitGroundFromAbove(Collision2D collision)
    {
        // 自分より下側との接触面（法線が上向き）だけを有効にする
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            if (contact.normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }
}
