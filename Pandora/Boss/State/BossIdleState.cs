using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BossIdleState : BossStateBase
{
    private float elapsedTime;
    public BossIdleState(BossContextBase bossContext, BossBattleState parent) : base(bossContext, parent) { }

    public override void Enter()
    {
        base.Enter();
        bossContext.PlayAnimIdle();
        elapsedTime = 0f;
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        elapsedTime += dt;
        if (elapsedTime > bossContext.idleTime)
        {
            parent.Change(((BossBattleState)parent).DecideAct());
        }
    }
}
