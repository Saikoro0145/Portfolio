using System.Collections.Generic;
using UnityEngine;

public class BossActionState : BossStateBase
{
    public BossActionState(BossContextBase bossContext, BossBattleState parent) : base(bossContext, parent){ }

    public override void Enter()
    {
        base.Enter();
        //parent.Change(((BossBattleState)parent).bossIdleState);
    }
}
