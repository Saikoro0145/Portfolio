using UnityEngine;

public sealed class PosBulletBarrage : BossActionState
{
    private enum Phase
    {
        SpawnOrbs,       // オーブ生成
        MovingOrbs,      // オーブ移動中
        DelayBeforeFire, // 弾発射前の待機
        Firing,          // 弾発射中（6回）
        PostFireDelay,   // 弾発射後の待機
        Finished         // 終了（Idleへ遷移）
    }

    private readonly GameObject orbPrefab;
    private readonly GameObject bulletPrefab;

    // 弾幕中心範囲（BarrageControllerの値をここに移植）
    private readonly float minX;
    private readonly float maxX;
    private readonly float minY;
    private readonly float maxY;

    // 攻撃設定
    private readonly Vector2 startPosition;    // オーブ出現位置
    private readonly float moveDuration;       // オーブの移動にかける時間
    private readonly float delayBeforeFire;    // 移動完了→発射開始までの待機
    private readonly float fireInterval;       // 何秒ごとに弾を撃つか（10f/60f）
    private readonly float postFireDelay;      // 最後の弾発射後の待機

    private Phase phase;
    private float timer;

    private GameObject orb1;
    private GameObject orb2;

    private Vector2 orb1Start;
    private Vector2 orb2Start;
    private Vector2 orb1Target;
    private Vector2 orb2Target;

    private int fireCount;                     // 何回目の発射か
    private readonly float[] angles = { 30f, 90f, 150f, 210f, 270f, 330f };

    public PosBulletBarrage(
        BossContextBase bossContext,
        BossBattleState parent,
        GameObject orbPrefab,
        GameObject bulletPrefab,
        float minX = -6f, float maxX = 6f,
        float minY = 3f, float maxY = 9f,
        float moveDuration = 2.0f,   // SmoothMovement.duration と同じ想定
        float delayBeforeFire = 1.0f,
        float fireInterval = 10f / 60f,
        float postFireDelay = 0.5f
    ) : base(bossContext, parent)
    {
        this.orbPrefab = orbPrefab;
        this.bulletPrefab = bulletPrefab;
        this.startPosition = bossContext.transform.position;

        this.minX = bossContext.transform.position.x + minX;
        this.maxX = bossContext.transform.position.x + maxX;
        this.minY = bossContext.transform.position.y + minY;
        this.maxY = bossContext.transform.position.y + maxY;

        this.moveDuration = moveDuration;
        this.delayBeforeFire = delayBeforeFire;
        this.fireInterval = fireInterval;
        this.postFireDelay = postFireDelay;
    }

    public override void Enter()
    {
        base.Enter();
        bossContext.animator.Play("Bullet");

        // 初期フェーズ設定
        phase = Phase.SpawnOrbs;
        timer = 0f;
        fireCount = 0;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        switch (phase)
        {
            case Phase.SpawnOrbs:
                HandleSpawnOrbs();
                break;

            case Phase.MovingOrbs:
                HandleMovingOrbs(dt);
                break;

            case Phase.DelayBeforeFire:
                HandleDelayBeforeFire(dt);
                break;

            case Phase.Firing:
                HandleFiring(dt);
                break;

            case Phase.PostFireDelay:
                HandlePostFireDelay(dt);
                break;

            case Phase.Finished:
                // Idle に戻す．PosTailSwing と同様に親ステートに遷移を依頼
                parent.Change(((BossBattleState)parent).bossIdleState);
                break;
        }
    }

    public override void Exit()
    {
        // オーブを破棄
        if (orb1 != null)
        {
            Object.Destroy(orb1);
            orb1 = null;
        }

        if (orb2 != null)
        {
            Object.Destroy(orb2);
            orb2 = null;
        }

        base.Exit();
    }

    // ---------- 各フェーズの処理 ----------

    private void HandleSpawnOrbs()
    {
        // オーブを2つ生成して初期位置に配置
        orb1 = Object.Instantiate(orbPrefab, startPosition, Quaternion.identity);
        orb2 = Object.Instantiate(orbPrefab, startPosition, Quaternion.identity);

        orb1Start = startPosition;
        orb2Start = startPosition;

        // ランダムターゲットを決定
        orb1Target = GetRandomPosition();
        orb2Target = GetRandomPosition();

        // 移動用タイマーをセット
        timer = moveDuration;

        phase = Phase.MovingOrbs;
    }

    private void HandleMovingOrbs(float dt)
    {
        if (orb1 == null || orb2 == null)
        {
            // 何らかの理由で消えていたら終了扱い
            phase = Phase.Finished;
            return;
        }

        timer -= dt;
        float progress = Mathf.Clamp01(1f - (timer / moveDuration));

        // Lerp で滑らかに移動
        Vector2 pos1 = Vector2.Lerp(orb1Start, orb1Target, progress);
        Vector2 pos2 = Vector2.Lerp(orb2Start, orb2Target, progress);

        orb1.transform.position = pos1;
        orb2.transform.position = pos2;

        if (timer <= 0f)
        {
            // 移動完了→発射前待機へ
            timer = delayBeforeFire;
            phase = Phase.DelayBeforeFire;
        }
    }

    private void HandleDelayBeforeFire(float dt)
    {
        timer -= dt;
        if (timer <= 0f)
        {
            // 弾発射フェーズへ
            fireCount = 0;
            timer = 0f; // すぐ1発目を撃つ
            phase = Phase.Firing;
        }
    }

    private void HandleFiring(float dt)
    {
        if (orb1 == null || orb2 == null)
        {
            phase = Phase.Finished;
            return;
        }

        timer -= dt;
        if (timer > 0f)
        {
            return;
        }

        // ここで1回分の弾発射（2つのオーブから6方向ずつ）
        FireBulletsFromOrb(orb1);
        FireBulletsFromOrb(orb2);


        fireCount++;
        if (fireCount >= 6)
        {
            // 全6回撃ち終わったので待機へ
            timer = postFireDelay;
            phase = Phase.PostFireDelay;
        }
        else
        {
            // 次の発射までのインターバルをセット
            timer = fireInterval;
        }
    }

    private void HandlePostFireDelay(float dt)
    {
        timer -= dt;
        if (timer <= 0f)
        {
            // 終了フェーズに移行（Idleへ戻す）
            phase = Phase.Finished;
        }
    }


    private void FireBulletsFromOrb(GameObject orb)
    {
        if (bulletPrefab == null) return;

        Vector2 origin = orb.transform.position;

        foreach (float angle in angles)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            //親を orb.transform に設定して生成
            GameObject bullet = Object.Instantiate(
                bulletPrefab,
                origin,
                Quaternion.Euler(0f, 0f, angle),
                orb.transform
            );
        }
    }


    private Vector2 GetRandomPosition()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        return new Vector2(x, y);
    }
}
