using UnityEngine;
using PixPlays.ElementalVFX;

public class PosLaser : BossActionState
{
    private enum Phase
    {
        DelayBeforeFire,
        FireAndRotate,
        DelayAfterFire,
        Finished
    }

    private readonly PosLaserRenderer laserRenderer;
    private readonly BaseVfx waterBeamPrefab;
    private readonly int waterBeamSortingLayerId;
    private readonly int waterBeamSortingOrder;

    private BaseVfx activeWaterBeam;
    private Transform beamTarget;
    private Transform groundTransform;

    private Phase phase;
    private float timer;
    private float fireElapsed;

    private float timeOfDelayBeforeFire = 30f / 60f;
    private float rotateDuration = 5f;
    private float timeOfDelayAfterFire = 0.5f;
    private float rightEdgeMargin = 2f;

    private float maxDistance = 30f;
    private Vector2 targetStart;
    private Vector2 targetEnd;

    public PosLaser(
        BossContextBase bossContext,
        BossBattleState parent,
        GameObject laser,
        GameObject waterBeam
    ) : base(bossContext, parent)
    {
        laserRenderer = laser.GetComponent<PosLaserRenderer>();
        waterBeamPrefab = waterBeam != null ? waterBeam.GetComponent<BaseVfx>() : null;
        var laserLineRenderer = laser.GetComponent<LineRenderer>();
        if (laserLineRenderer != null)
        {
            waterBeamSortingLayerId = laserLineRenderer.sortingLayerID;
            waterBeamSortingOrder = laserLineRenderer.sortingOrder;
        }
        else
        {
            waterBeamSortingLayerId = SortingLayer.NameToID("Default");
            waterBeamSortingOrder = 10;
        }

        var stage = bossContext.stage;
        if (stage == null)
        {
            Debug.LogError("PosLaser: stage is null.");
            return;
        }

        var ground = stage.transform.Find("Ground");
        if (ground == null)
        {
            Debug.LogError("PosLaser: Ground object was not found under stage.");
            return;
        }

        groundTransform = ground;
    }

    public override void Enter()
    {
        base.Enter();
        bossContext.animator.Play("Beam");

        laserRenderer.Init(maxDistance);
        laserRenderer.SetVisible(true);
        laserRenderer.SetLineVisible(waterBeamPrefab == null);

        if (!TryGetBeamTargetRange(out targetStart, out targetEnd))
        {
            Vector2 origin = laserRenderer.transform.position;
            targetStart = origin + Vector2.left * 5f;
            targetEnd = origin + Vector2.right * 5f;
        }

        phase = Phase.DelayBeforeFire;
        timer = timeOfDelayBeforeFire;
        fireElapsed = 0f;
    }

    public override void Update(float dt)
    {
        base.Update(dt);

        switch (phase)
        {
            case Phase.DelayBeforeFire:
                PhaseDelayBeforeFire(dt);
                break;

            case Phase.FireAndRotate:
                PhaseFireAndRotate(dt);
                break;

            case Phase.DelayAfterFire:
                PhaseDelayAfterFire(dt);
                break;

            case Phase.Finished:
                parent.Change(((BossBattleState)parent).bossIdleState);
                break;
        }
    }

    public override void Exit()
    {
        StopWaterBeam();
        laserRenderer.SetVisible(false);

        base.Exit();
    }

    private void PhaseDelayBeforeFire(float dt)
    {
        timer -= dt;
        if (timer > 0f) return;

        phase = Phase.FireAndRotate;
    }

    private void PhaseFireAndRotate(float dt)
    {
        float safeDuration = Mathf.Max(rotateDuration, 0.0001f);
        Vector2 target = Vector2.Lerp(targetStart, targetEnd, Mathf.Clamp01(fireElapsed / safeDuration));
        Vector2 origin = laserRenderer.transform.position;
        Vector2 dir = target - origin;

        if (dir.sqrMagnitude < 0.0001f)
        {
            dir = Vector2.up;
        }

        Vector2 end = laserRenderer.Render(origin, dir);

        StartWaterBeamIfNeeded(end);
        UpdateWaterBeamTarget(end);

        fireElapsed += dt;
        if (fireElapsed >= safeDuration)
        {
            phase = Phase.DelayAfterFire;
            timer = timeOfDelayAfterFire;
        }
    }

    private void PhaseDelayAfterFire(float dt)
    {
        timer -= dt;
        if (timer > 0f) return;

        phase = Phase.Finished;
    }

    private void StartWaterBeamIfNeeded(Vector2 end)
    {
        if (waterBeamPrefab == null || activeWaterBeam != null) return;

        GameObject targetObject = new GameObject("PosLaserBeamTarget");
        beamTarget = targetObject.transform;
        UpdateWaterBeamTarget(end);

        activeWaterBeam = Object.Instantiate(waterBeamPrefab);
        ApplyWaterBeamSorting(activeWaterBeam.gameObject);
        float beamDuration = rotateDuration + timeOfDelayAfterFire + 0.2f;
        activeWaterBeam.Play(new VfxData(laserRenderer.transform, beamTarget, beamDuration, 0.5f));
    }

    private void UpdateWaterBeamTarget(Vector2 end)
    {
        if (beamTarget == null) return;
        beamTarget.position = new Vector3(end.x, end.y, laserRenderer.transform.position.z);
    }

    private void StopWaterBeam()
    {
        if (activeWaterBeam != null)
        {
            activeWaterBeam.Stop();
            activeWaterBeam = null;
        }

        if (beamTarget != null)
        {
            Object.Destroy(beamTarget.gameObject);
            beamTarget = null;
        }
    }

    private void ApplyWaterBeamSorting(GameObject beamRoot)
    {
        if (beamRoot == null) return;

        Renderer[] renderers = beamRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingLayerID = waterBeamSortingLayerId;
            renderers[i].sortingOrder = waterBeamSortingOrder;
        }
    }

    private bool TryGetBeamTargetRange(out Vector2 start, out Vector2 end)
    {
        start = Vector2.zero;
        end = Vector2.zero;

        if (groundTransform == null)
        {
            Debug.LogError("PosLaser: groundTransform is null.");
            return false;
        }

        Vector3 center = groundTransform.position;
        Vector3 scale = groundTransform.lossyScale;
        float halfWidth = Mathf.Abs(scale.x) * 0.5f;
        float halfHeight = Mathf.Abs(scale.y) * 0.5f;
        float minX = center.x - halfWidth;
        float maxX = center.x + halfWidth;
        float topY = center.y + halfHeight;
        float clampedMargin = Mathf.Clamp(rightEdgeMargin, 0f, maxX - minX);

        start = new Vector2(minX, topY);
        end = new Vector2(maxX - clampedMargin, topY);
        return true;
    }
}
