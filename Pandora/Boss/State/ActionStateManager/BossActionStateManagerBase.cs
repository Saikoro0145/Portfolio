using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class BossActionStateManagerBase : MonoBehaviour
{
    protected List<BossActionState>[] actionSMPickPools;
    public int PhaseCount => actionSMPickPools == null ? 0 : actionSMPickPools.Length;

    public List<BossActionState> GetActionStatePickPool(int indexOfPhase)
    {
        if (indexOfPhase < 0 || indexOfPhase >= actionSMPickPools.Length)
        {
            Debug.LogWarning("Index of phase is out of range.");
            return null;
        }

        //Debug.Log(string.Join(", ", actionSMPickPools[indexOfPhase].Select(c => c.GetType().Name)));
        return actionSMPickPools[indexOfPhase];
    }

    public abstract void InitializeActionStatePickPools(BossContextBase bossContext, BossBattleState bossBattleState);
}
