using System.Collections.Generic;
using UnityEngine;

public class BossBattleState: BossStateBase
{
    public BossIdleState bossIdleState;
    private int currentPhase;
    private List<BossActionState> actionStatePickPool;
    public BossBattleState(BossContextBase bossContext, BossSM parent) : base(bossContext, parent) 
    {
        bossContext.bossActionStateManager.InitializeActionStatePickPools(bossContext, this);
        bossIdleState = new BossIdleState(bossContext, this);
        currentPhase = -1;
        actionStatePickPool = bossContext.bossActionStateManager.GetActionStatePickPool(++currentPhase);
    }

    public override void Enter()
    {
        base.Enter();
        Change(bossIdleState);
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        if (bossContext.bossController.health.currentHP <= 0)
        {
            parent.Change(((BossSM)parent).bossDieState);
        }
    }

    public BossStateBase DecideAct()
    {
        if (currentPhase < bossContext.thresholdsOfPhase.Length &&
            bossContext.bossController.health.currentHP <= bossContext.thresholdsOfPhase[currentPhase])
        {
            int nextPhase = currentPhase + 1;
            if (nextPhase < bossContext.bossActionStateManager.PhaseCount)
            {
                var nextPool = bossContext.bossActionStateManager.GetActionStatePickPool(nextPhase);
                if (nextPool != null && nextPool.Count > 0)
                {
                    currentPhase = nextPhase;
                    actionStatePickPool = nextPool;
                }
            }
        }

        if (actionStatePickPool == null || actionStatePickPool.Count == 0)
        {
            Debug.LogError("Action pick pool is empty or null.");
            return null;
        }

        int index = Random.Range(0, actionStatePickPool.Count);
        return actionStatePickPool[index];
    }
}
