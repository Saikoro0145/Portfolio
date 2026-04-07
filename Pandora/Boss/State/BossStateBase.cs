using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class BossStateBase : BossStateAsParent, IBossState
{
    protected BossContextBase bossContext { get; private set; }
    protected Stack<BossStateBase> bossStateStack;
    protected BossStateAsParent parent;

    private protected readonly Animator animator;


    protected BossStateBase(BossContextBase bossContext, BossStateAsParent parent) 
    {
        this.bossContext = bossContext;
        this.animator = bossContext.animator;

        if (parent is BossSM bossSM)
        {
            this.bossStateStack = bossSM.bossStateStack;
        }
        else // parent is BossStateBase
        {
            this.bossStateStack = ((BossStateBase)parent).bossStateStack;
        }
        AttachTo(parent);
    }

    public void AttachTo(BossStateAsParent parent) 
    {
        this.parent?.children.Remove(this);
        this.parent = parent;
        parent.children.Add(this); 
    }

    //継承先のクラスでは原則base.Enter()をEnter()の最初に呼び出すこと．
    public virtual void Enter() 
    {
        Debug.Log($" BossStateEnter: {GetType().Name}");
        bossStateStack.Push(this);
        //Debug.Log(string.Join(", ", bossStateStack.Reverse().Select(sm => sm.GetType().Name)));
    }

    //継承先のクラスでは"必ず"base.Update()をUpadate()の最初に呼び出すこと．
    public virtual void Update(float dt) {}

    //継承先のクラスでは原則base.Exit()をExit()の最""後""に呼び出すこと
    public virtual void Exit() 
    {
        bossStateStack.Pop();
        //Debug.Log(string.Join(", ", bossStateStack.Reverse().Select(sm => sm.GetType().Name)));
        Debug.Log($" BossStateExit: {GetType().Name}");
    }

    public override void Change(BossStateBase child)
    {
        if (child == null || !children.Contains(child)) { 
            Debug.LogError("child == null || !children.Contains(child)");
            return;
        }
        // 自分までの状態を抜けていく
        while (bossStateStack.Peek() != this)
        {
            bossStateStack.Peek().Exit();
        }
        child.Enter();
    }
}