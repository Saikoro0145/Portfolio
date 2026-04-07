using UnityEngine;

public class PosLaserRenderer : MonoBehaviour
{
    private LineRenderer line;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private LayerMask stopLayers;

    [SerializeField] private GameObject hitBox;
    [SerializeField] private float hitboxThickness = 0.3f;
    private Transform hbTransform;
    private CapsuleCollider2D hbCollider;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        hbTransform = hitBox.transform;
        hbCollider = hitBox.GetComponent<CapsuleCollider2D>();
        hbCollider.direction = CapsuleDirection2D.Horizontal;
        SetVisible(false);
    }

    public void Init(float maxDistance)
    {
        this.maxDistance = maxDistance;

        line.positionCount = 2;
        Vector2 resetPoint = transform.position;
        line.SetPosition(0, new Vector3(resetPoint.x, resetPoint.y, 0f));
        line.SetPosition(1, new Vector3(resetPoint.x, resetPoint.y, 0f));
        UpdateHitbox(resetPoint, resetPoint);
        line.enabled = false;
    }

    public void SetVisible(bool visible)
    {
        if (line != null) line.enabled = visible;
        if (hbCollider != null) hbCollider.enabled = visible;
    }

    public void SetLineVisible(bool visible)
    {
        if (line != null) line.enabled = visible;
    }

    public Vector2 Render(Vector2 start, Vector2 dir)
    {
        if (line == null) return start;

        Vector2 normalizedDir = dir.sqrMagnitude > 0f ? dir.normalized : Vector2.up;
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, normalizedDir, maxDistance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit2D nearestValidHit = default;
        bool found = false;

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.collider == null) continue;

            int layerBit = 1 << hit.collider.gameObject.layer;
            if ((stopLayers.value & layerBit) != 0 || hit.collider.CompareTag("Player"))
            {
                nearestValidHit = hit;
                found = true;
                break;
            }
        }

        Vector2 end = found ? nearestValidHit.point : start + normalizedDir * maxDistance;

        line.SetPosition(0, new Vector3(start.x, start.y, 0f));
        line.SetPosition(1, new Vector3(end.x, end.y, 0f));
        UpdateHitbox(start, end);
        return end;
    }

    private void UpdateHitbox(Vector2 start, Vector2 end)
    {
        Vector2 delta = end - start;
        float length = delta.magnitude;
        float safeLen = Mathf.Max(length, 0.001f);

        float angleDeg = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        hbTransform.position = start;
        hbTransform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        hbCollider.direction = CapsuleDirection2D.Horizontal;
        hbCollider.offset = new Vector2(safeLen * 0.5f, 0f);
        hbCollider.size = new Vector2(safeLen, hitboxThickness);
    }
}
