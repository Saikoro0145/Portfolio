using UnityEngine;

public class BossBornState : BossStateBase
{
    public BossBornState(BossContextBase bossContext, BossSM parent) : base(bossContext, parent){ }

    public override void Enter()
    {
        base.Enter();
        // ボスの出現処理をここに追加
        bossContext.PlayAnimBorn();

    }

    public override void Update(float dt)
    {
        base.Update(dt);

        if (bossContext.IsAnimBornFinished())
        {
            parent.Change(((BossSM)parent).bossBattleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
