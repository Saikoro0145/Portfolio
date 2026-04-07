using UnityEngine;

public sealed class PosTailSwing : BossActionState
{
    private enum Phase
    {
        DelayBeforeSpawn,
        PostSpawnDelay,
        Moving,
        PostMoveDelay,
        Shrink,
        PostDestroyDelay
    }

    private readonly GameObject tail;
    private readonly float speed;
    private readonly float moveDistance;
    private float spownHeight;
    private readonly float smallS;

    private Phase phase;
    private float timer;
    private float timeOfDelayBeforeSpawn;
    private float timeOfPostSpawnDelay;
    private float timeOfPostMoveDelay;
    private float timeOfPostDestroyDelay;

    private Vector3 targetPosition;

    private GameObject newObj;
    private BoxCollider2D boxCollider;

    public PosTailSwing(
        BossContextBase bossContext,
        BossBattleState parent,
        GameObject tailSource,
        float speed = 8.0f,
        float moveDistance = 12f,
        float spownHeight = 2.0f,
        float smallS = 12f
    ) : base(bossContext, parent)
    {
        tail = tailSource;
        this.speed = speed;
        this.moveDistance = moveDistance;
        this.spownHeight = spownHeight;
        this.smallS = smallS;
        timeOfDelayBeforeSpawn = 20f / 60f;
        timeOfPostSpawnDelay = 10f / 60f;
        timeOfPostMoveDelay = 5f / 60f;
        timeOfPostDestroyDelay = 10f / 60f;

    }

    public override void Enter()
    {
        base.Enter();
        bossContext.animator.Play("Tail");
        phase = Phase.DelayBeforeSpawn;
        timer = timeOfDelayBeforeSpawn;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        switch (phase)
        {
            case Phase.DelayBeforeSpawn:
                {
                    UpdateAndCheckTimer(dt, Phase.PostSpawnDelay);
                    if (timer <= 0f)
                    {
                        //尻尾オブジェクトのインスタンス化
                        var spownPosition = bossContext.stage.transform.position + new Vector3(-moveDistance / 2f, spownHeight, 0f);
                        targetPosition = spownPosition + new Vector3(moveDistance, 0f, 0f);
                        newObj = Object.Instantiate(tail, spownPosition, Quaternion.identity, bossContext.stage.gameObject.transform);
                        boxCollider = newObj.GetComponent<BoxCollider2D>();

                        timer = timeOfPostSpawnDelay;
                    }
                    break;
                }

            case Phase.PostSpawnDelay:
                {
                    UpdateAndCheckTimer(dt, Phase.Moving);
                    break;
                }

            case Phase.Moving:
                {
                    // 尻尾オブジェクトの移動
                    newObj.transform.position = Vector3.MoveTowards(
                        newObj.transform.position,
                        targetPosition,
                        speed * dt
                    );

                    // 目的地（targetPosition）にどれだけ近づいたかを見る
                    float distanceToTarget = Vector3.Distance(newObj.transform.position, targetPosition);

                    // ある程度近づいたら次のフェーズへ
                    if (distanceToTarget <= 0.01f)
                    {
                        timer = timeOfPostMoveDelay;
                        phase = Phase.PostMoveDelay;
                    }

                    //Debug.LogWarning($"Moving Phase, distanceToTarget = {distanceToTarget}");
                    break;
                }

            case Phase.PostMoveDelay:
                {
                    UpdateAndCheckTimer(dt, Phase.Shrink);
                    //Debug.LogWarning("PostMoveDelay Phase");
                    break;
                }

            case Phase.Shrink:
                {
                    //尻尾オブジェクトの縮小
                    boxCollider.size = new Vector2(boxCollider.size.x - (smallS * Time.deltaTime), boxCollider.size.y - (smallS * Time.deltaTime));

                    if (boxCollider.size.x <= 0.01f)
                    {
                        timer = timeOfPostDestroyDelay;
                        phase = Phase.PostDestroyDelay;
                    }
                    //Debug.LogWarning("Shrink Phase");
                    break;
                }

            case Phase.PostDestroyDelay:
                {
                    UpdateAndCheckTimer(dt, null);
                    parent.Change(((BossBattleState)parent).bossIdleState);
                    //Debug.LogWarning("PostDestroyDelay Phase");
                    break;
                }
        }
    }

    public override void Exit()
    {
        Object.Destroy(newObj);
        newObj = null;
        boxCollider = null;
        base.Exit();
    }

    private void UpdateAndCheckTimer(float dt, Phase? nextPhase)
    {
        timer -= dt;
        if (timer <= 0f && nextPhase.HasValue)
        {
            phase = nextPhase.Value;
        }
    }
}
