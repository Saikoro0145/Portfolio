using UnityEngine;

public class PoseidonContext : BossContextBase
{
    public void PlayAnimBubble()
    {
        animator.Play("Bubble");
    }

    public void PlayAnimBullet()
    {
        animator.Play("Bullet");
    }

    public void PlayAnimBeam()
    {
        animator.Play("Beam");
    }

    public void PlayAnimTail()
    {
        animator.Play("Tail");
    }


    public bool IsAnimBubbleFinished() {
        return IsStateFinished("Bubble");
    }

    public bool IsAnimBulletFinished()
    {
        return IsStateFinished("Bullet");
    }
    public bool IsAnimBeamFinished()
    {
        return IsStateFinished("Beam");
    }
    public bool IsAnimTailFinished()
    {
        return IsStateFinished("Tail");
    }
}
