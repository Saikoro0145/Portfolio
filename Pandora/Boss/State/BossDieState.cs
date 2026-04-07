using UnityEngine;

public class BossDieState : BossStateBase
{
    public BossDieState(BossContextBase bossContext, BossSM parent) : base(bossContext, parent) { }

    public override void Enter()
    {
        base.Enter();
        bossContext.PlayAnimDie();
    }
}
