using System.Collections.Generic;
using UnityEngine;

public class BossSM : BossStateAsParent
{
    public Stack<BossStateBase> bossStateStack;

    public BossBornState bossBornState;
    public BossBattleState bossBattleState;
    public BossDieState bossDieState;

    public BossSM(BossContextBase bossContext)
    {
        bossStateStack = new Stack<BossStateBase>();
        bossBornState = new BossBornState(bossContext, this);
        bossBattleState = new BossBattleState(bossContext, this);
        bossDieState = new BossDieState(bossContext, this);
    }

    public override void Change(BossStateBase child)
    {
        if (child == null || !children.Contains(child)) { Debug.LogError("child == null || !children.Contains(child)"); }
        while (bossStateStack.Count > 0)
        {
            bossStateStack.Peek().Exit();
        }
        child.Enter();
    }

    public void UpdateAllStates(float dt)
    {
        var arr = bossStateStack.ToArray();

        for (int i = arr.Length - 1; i >= 0; i--)
        {
            arr[i].Update(dt);
        }
    }
}
