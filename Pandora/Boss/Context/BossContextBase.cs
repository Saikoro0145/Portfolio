using System.Collections.Generic;
using UnityEngine;

public class BossContextBase : MonoBehaviour
{
    [SerializeField] public int[] thresholdsOfPhase  = new int[] { 5, 0 };
    [SerializeField] public float idleTime = 2.5f;
    public Animator animator;
    public BossActionStateManagerBase bossActionStateManager { get; private set; }
    public BossController bossController { get; private set; }

    public GameObject stage { get; private set; }

    public void Awake()
    {
        animator = GetComponent<Animator>();
        bossActionStateManager = GetComponent<BossActionStateManagerBase>();
        bossController = GetComponent<BossController>();
        stage = transform.parent.gameObject;
    }

    public void PlayAnimBorn()
    {
        animator.Play("Born");
    }

    public void PlayAnimIdle()
    {
        animator.Play("Idle");
    }

    public void PlayAnimDie()
    {
        animator.Play("Die");
    }

    public bool IsAnimBornFinished()
    {
        return IsStateFinished("Born");
    }

    public bool IsAnimDieFinished()
    {
        return IsStateFinished("Die");
    }

    public bool IsStateFinished(string stateName, int layer = 0)
    {
        var info = animator.GetCurrentAnimatorStateInfo(layer);

        if (!info.IsName(stateName)) return false;
        if (animator.IsInTransition(layer)) return false;
        return info.normalizedTime >= 1f;
    }
}
