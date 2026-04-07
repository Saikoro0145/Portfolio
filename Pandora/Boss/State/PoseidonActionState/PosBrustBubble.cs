using UnityEngine;

public class PosBrustBubble : BossActionState
{
    private enum Phase
    {
        DelayBeforeSpown, // 生成前の待機,アニメーション用
        SpawnBubble,       // オーブ生成]
        DelayAfterSpown, // 生成後の待機,アニメーション用
        Finished         // 終了（Idleへ遷移）
    }

    private GameObject bubbleSource;
    private Phase phase;
    private float timer;
    private float timeOfDelayBeforeSpown = 30f / 60f;
    private float timeOfDelayAfterSpown = 1.5f;
    private int numOfBubbles = 3;

    public PosBrustBubble(BossContextBase bossContext, BossBattleState parent, GameObject bubbleSource) : base(bossContext, parent)
    {
        this.bubbleSource = bubbleSource;
    }

    public override void Enter()
    {
        base.Enter();
        bossContext.animator.Play("Bubble");
        phase = Phase.DelayBeforeSpown;
        timer = timeOfDelayBeforeSpown;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        switch (phase)
        {
            case Phase.DelayBeforeSpown:
                timer -= dt;
                if (timer <= 0f)
                {
                    phase = Phase.SpawnBubble;
                }
                break;
            case Phase.SpawnBubble:
                GameObject stage = bossContext.stage;
                GameObject ground = stage.transform.Find("Ground").gameObject;
                if (ground == null)
                {
                    Debug.LogError("Groundオブジェクトが見つかりません。");
                    return;
                }

                float gWidth = ground.transform.localScale.x;
                float lower = -gWidth / 2f;
                float range = gWidth / numOfBubbles;
                float upper = lower + range;

                for (int i = 0; i < numOfBubbles; i++)
                { 
                    float offset = Random.Range(lower + i * range, upper + i * range);
                    float x = stage.transform.position.x + offset;
                    float y = stage.transform.position.y - 1f;

                    Object.Instantiate(
                        bubbleSource,
                        new Vector3(x, y, 0f),
                        Quaternion.identity,
                        stage.gameObject.transform
                    );
                }
                phase = Phase.DelayAfterSpown;
                timer = timeOfDelayAfterSpown;
                break;
            case Phase.DelayAfterSpown:
                timer -= dt;
                if (timer <= 0f)
                {
                    phase = Phase.Finished;
                }
                break;
            case Phase.Finished:
                parent.Change(((BossBattleState)parent).bossIdleState);
                break;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
