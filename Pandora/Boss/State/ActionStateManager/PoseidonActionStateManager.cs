using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class PoseidonActionStateManager : BossActionStateManagerBase
{
    [Header("Bullet Barrage")]
    [SerializeField] GameObject orb;
    [SerializeField] GameObject bullet;

    [Header("Tail Swing")]
    [SerializeField] GameObject tail;

    [Header("Brust Bubble")]
    [SerializeField] GameObject bubble;

    [Header("Laser")]
    [SerializeField] GameObject laser;
    [SerializeField] GameObject waterBeam;

    public PosBulletBarrage posBulletBarrage;
    public PosTailSwing posTailSwing;
    public PosBrustBubble posBrustBubble;
    public PosLaser posLaser;

    public override void InitializeActionStatePickPools(BossContextBase bossContext, BossBattleState bossBattleState)
    {
        posBulletBarrage  = new PosBulletBarrage(bossContext, bossBattleState, orb, bullet);
        posTailSwing = new PosTailSwing(bossContext, bossBattleState, tail);
        posBrustBubble = new PosBrustBubble(bossContext, bossBattleState, bubble);
        posLaser = new PosLaser(bossContext, bossBattleState, laser, waterBeam);

        this.actionSMPickPools = new List<BossActionState>[]
        {
            // Phase 0
            new List<BossActionState>
            {
                //posBulletBarrage,
                //posTailSwing,
                //posBrustBubble
                posLaser
            },
            // Phase 1
            new List<BossActionState>
            {
                //posBulletBarrage,
                //posTailSwing,
                //posBrustBubble,
                posLaser
            },
        };
    }
}
