using UnityEngine;

public class PosBulletController : MonoBehaviour
{
    // 弾丸の移動速度
    [SerializeField] public float speed = 10f;
    // 弾丸の生存時間
    [SerializeField] public float lifetime = 5f;
    //弾が衝突した際弾を破壊するレイヤー
    [SerializeField] private LayerMask destroyLayers;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 弾丸の速度を設定
        rb.linearVelocity = transform.right * speed;
        // 指定した時間後に弾丸を破壊
        Destroy(gameObject, lifetime);
    }

    // 衝突時の処理
    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log($"衝突: {other.gameObject.name}");

        if (((1 << other.gameObject.layer) & destroyLayers) != 0 || other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}