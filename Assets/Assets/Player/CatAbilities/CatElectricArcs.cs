using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum CatAbilityType
{
    CatVision,
    CatHearing,
    CatClaw
}

/// <summary>
/// Procedural electric arcs that follow an ability mesh's silhouette.
/// Keeps the ability transform/position unchanged.
/// Assign an additive URP Unlit material to Arc Material.
/// </summary>
[DisallowMultipleComponent]
public class CatElectricArcs : MonoBehaviour
{
    private static readonly int GlowColorProperty = Shader.PropertyToID("_GlowColor");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private enum PathShape
    {
        EarTriangle,
        Ring,
        Paw
    }

    [Header("References")]
    [FormerlySerializedAs("earRenderer")]
    [SerializeField] private Renderer abilityRenderer;
    [SerializeField] private Material arcMaterial;

    [Header("Ability selection")]
    [SerializeField] private CatAbilityType abilityType;
    [SerializeField] private Color selectedYellow = new Color(1f, 0.72f, 0.05f, 1f);

    [Header("Path")]
    [SerializeField] private PathShape pathShape = PathShape.EarTriangle;
    [SerializeField, Range(0.5f, 1f)] private float ringRadius = 0.85f;
    [SerializeField, Range(12, 64)] private int ringSegments = 32;
    [SerializeField, Range(0f, 8f)] private float flowSpeed = 0f;
    [SerializeField] private bool hideSourceMesh = true;

    [Header("Electric look")]
    [SerializeField, Range(3, 12)] private int segmentsPerSide = 7;
    [SerializeField, Range(0.005f, 0.12f)] private float jaggedness = 0.005f;
    [SerializeField, Range(0.005f, 0.08f)] private float coreWidth = 0.005f;
    [SerializeField, Range(0.02f, 0.2f)] private float auraWidth = 0.025f;
    [SerializeField, Range(0, 8)] private int branchCount = 3;
    [SerializeField, Range(0.02f, 0.3f)] private float branchLength = 0.045f;
    [SerializeField, Range(0.03f, 0.25f)] private float refreshInterval = 0.075f;
    [SerializeField] private Color electricBlue = new Color(0.08f, 0.55f, 1f, 1f);

    private PathShape ActivePathShape =>
        abilityType == CatAbilityType.CatClaw ? PathShape.Paw : pathShape;
    private int PawSparkCount => Mathf.Min(branchCount, 2);

    private readonly List<LineRenderer> lines = new List<LineRenderer>();
    private float nextRefresh;
    private Bounds localBounds;
    private bool sourceRendererWasEnabled;
    private bool sourceRendererStateCaptured;
    private bool abilityModeVisible;
    private Color activeEnergyColor;
    private MaterialPropertyBlock originalSourceProperties;

    public CatAbilityType AbilityType => abilityType;

    private void Awake()
    {
        if (abilityRenderer == null)
        {
            Renderer[] childRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer candidate in childRenderers)
            {
                if (candidate is ParticleSystemRenderer
                    || candidate is LineRenderer
                    || candidate is TrailRenderer)
                    continue;

                abilityRenderer = candidate;
                break;
            }
        }

        if (abilityRenderer == null)
        {
            Debug.LogWarning("CatElectricArcs: asigna Ability Renderer.", this);
            enabled = false;
            return;
        }

        if (abilityRenderer is ParticleSystemRenderer)
        {
            Debug.LogError("CatElectricArcs: Ability Renderer debe ser una malla, no un Particle System Renderer.", this);
            enabled = false;
            return;
        }

        sourceRendererWasEnabled = abilityRenderer.enabled;
        sourceRendererStateCaptured = true;
        originalSourceProperties = new MaterialPropertyBlock();
        abilityRenderer.GetPropertyBlock(originalSourceProperties);
        if (hideSourceMesh)
            abilityRenderer.enabled = false;

        if (arcMaterial == null)
        {
            Debug.LogWarning("CatElectricArcs: asigna Arc Material.", this);
            enabled = false;
            return;
        }

        MeshFilter mf = abilityRenderer.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
            localBounds = mf.sharedMesh.bounds;
        else
            localBounds = abilityRenderer.localBounds;

        int outlineCount = ActivePathShape == PathShape.Paw ? 5 : ActivePathShape == PathShape.Ring ? 1 : 3;
        int total = ActivePathShape == PathShape.Paw
            ? outlineCount + PawSparkCount
            : 3 * (outlineCount + branchCount);
        for (int i = 0; i < total; i++)
        {
            GameObject go = new GameObject("ElectricArc_" + i);
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.positionCount = 2;
            lr.material = arcMaterial;
            lr.enabled = false;
            lr.numCapVertices = 2;
            lr.numCornerVertices = 2;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            lines.Add(lr);
        }
    }

    private void LateUpdate()
    {
        if (abilityModeVisible && Time.time >= nextRefresh)
        {
            Regenerate();
            nextRefresh = Time.time + refreshInterval;
        }
    }

    public void SetAbilityMode(bool visible, CatAbilityType selectedAbility)
    {
        if (abilityRenderer == null)
            return;

        bool wasVisible = abilityModeVisible;
        Color nextColor = abilityType == selectedAbility ? selectedYellow : electricBlue;
        bool colorChanged = activeEnergyColor != nextColor;

        abilityModeVisible = visible;
        activeEnergyColor = nextColor;
        abilityRenderer.enabled = visible && !hideSourceMesh && sourceRendererWasEnabled;
        if (abilityRenderer.enabled)
        {
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            abilityRenderer.GetPropertyBlock(properties);
            properties.SetColor(GlowColorProperty, activeEnergyColor);
            properties.SetColor(BaseColorProperty, activeEnergyColor);
            properties.SetColor(ColorProperty, activeEnergyColor);
            abilityRenderer.SetPropertyBlock(properties);
        }

        if (!visible)
        {
            foreach (LineRenderer line in lines)
                line.enabled = false;
            return;
        }

        if (!wasVisible || colorChanged)
        {
            Regenerate();
            nextRefresh = Time.time + refreshInterval;
        }
    }

    private Vector3 Point(float x, float z, float depth)
    {
        // Ear FBX supplied with this project uses local Z as its height axis.
        return abilityRenderer.transform.TransformPoint(new Vector3(x, depth, z));
    }

    private void ConfigureLine(LineRenderer lr, float width, Color color, float alpha, bool closed)
    {
        lr.startWidth = width;
        lr.endWidth = closed ? width : width * 0.08f;
        Color c = color; c.a = alpha;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
            closed
                ? new[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(alpha, 1f) }
                : new[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(0f, 1f) });
        lr.colorGradient = g;
        lr.enabled = true;
    }

    private void DrawLayeredArc(Vector3[] points, ref int lineIndex, bool closed = false)
    {
        if (points == null || points.Length < 2) return;

        LineRenderer aura = lines[lineIndex++];
        aura.positionCount = points.Length;
        aura.SetPositions(points);
        ConfigureLine(aura, auraWidth, activeEnergyColor, 0.16f, closed);

        LineRenderer body = lines[lineIndex++];
        body.positionCount = points.Length;
        body.SetPositions(points);
        ConfigureLine(body, coreWidth * 2.2f, activeEnergyColor, 0.9f, closed);

        LineRenderer core = lines[lineIndex++];
        core.positionCount = points.Length;
        core.SetPositions(points);
        ConfigureLine(core, coreWidth * 0.7f, Color.Lerp(activeEnergyColor, Color.white, 0.65f), 1f, closed);
    }

    private Vector3[] JaggedPath(Vector3 a, Vector3 b, int segments, float amount)
    {
        Vector3[] pts = new Vector3[segments + 1];
        pts[0] = a;
        pts[segments] = b;
        Transform geometryTransform = abilityRenderer.transform;
        Vector3 side = Vector3.Cross((b - a).normalized, geometryTransform.forward).normalized;
        if (side.sqrMagnitude < 0.001f) side = geometryTransform.right;

        for (int i = 1; i < segments; i++)
        {
            float t = i / (float)segments;
            Vector3 p = Vector3.Lerp(a, b, t);
            p += side * Random.Range(-amount, amount);
            p += geometryTransform.forward * Random.Range(-amount * 0.35f, amount * 0.35f);
            pts[i] = p;
        }
        return pts;
    }

    private void Regenerate()
    {
        if (lines.Count == 0) return;

        float depth = localBounds.center.y - localBounds.extents.y * 0.55f;
        int idx = 0;

        if (ActivePathShape == PathShape.Paw)
        {
            Vector3[][] paw = JaggedPawPaths();
            for (int i = 0; i < paw.Length; i++)
                DrawPawOutline(paw[i], ref idx, i);

            Vector3 center = abilityRenderer.transform.TransformPoint(localBounds.center);
            for (int i = 0; i < PawSparkCount; i++)
            {
                Vector3[] pad = paw[Random.Range(0, paw.Length)];
                Vector3 start = pad[Random.Range(0, ringSegments)];
                Vector3 radial = (start - center).normalized;
                Vector3 side = Vector3.Cross(radial, abilityRenderer.transform.forward).normalized;
                Vector3 direction = (radial + side * Random.Range(-0.4f, 0.4f)).normalized;
                float maxLength = Mathf.Max(
                    0.005f, Mathf.Min(branchLength, abilityRenderer.bounds.extents.magnitude * 0.18f));
                Vector3 end = start + direction * Random.Range(maxLength * 0.45f, maxLength);
                Vector3[] spark = JaggedPath(
                    start, end, Random.Range(2, 5), Mathf.Min(jaggedness, branchLength * 0.15f));
                DrawPawSpark(spark, ref idx);
            }
        }
        else if (ActivePathShape == PathShape.Ring)
        {
            Vector3[] ring = JaggedRingPath();
            DrawLayeredArc(ring, ref idx, true);

            for (int i = 0; i < branchCount; i++)
            {
                int segment = Random.Range(0, ringSegments);
                Vector3 a = ring[segment];
                Vector3 radial = (a - abilityRenderer.transform.TransformPoint(localBounds.center)).normalized;
                Vector3 b = a + radial * Random.Range(branchLength * 0.45f, branchLength);
                DrawLayeredArc(JaggedPath(a, b, Random.Range(2, 5), jaggedness * 0.8f), ref idx);
            }
        }
        else
        {
            float minX = localBounds.min.x;
            float maxX = localBounds.max.x;
            float minZ = localBounds.min.z;
            float maxZ = localBounds.max.z;

            // Approximate the ear's triangular silhouette using its local bounds.
            Vector3 tip = Point((minX + maxX) * 0.5f, maxZ, depth);
            Vector3 left = Point(minX, minZ, depth);
            Vector3 right = Point(maxX, minZ, depth);
            Vector3[] corners = { left, tip, right, left };

            for (int side = 0; side < 3; side++)
                DrawLayeredArc(JaggedPath(corners[side], corners[side + 1],
                    segmentsPerSide, jaggedness), ref idx);

            for (int i = 0; i < branchCount; i++)
            {
                int side = Random.Range(0, 3);
                Vector3 a = Vector3.Lerp(corners[side], corners[side + 1], Random.Range(0.2f, 0.85f));
                Vector3 direction = (a - abilityRenderer.transform.TransformPoint(localBounds.center)).normalized;
                Vector3 b = a + direction * Random.Range(branchLength * 0.45f, branchLength);
                DrawLayeredArc(JaggedPath(a, b, Random.Range(2, 5), jaggedness * 0.8f), ref idx);
            }
        }

        for (; idx < lines.Count; idx++)
            lines[idx].enabled = false;
    }

    private Vector3[][] JaggedPawPaths()
    {
        Vector3[][] paths = new Vector3[5][];
        Vector3 extents = localBounds.extents;
        int depthAxis = 0;
        if (extents.y < extents[depthAxis]) depthAxis = 1;
        if (extents.z < extents[depthAxis]) depthAxis = 2;

        int axisA = (depthAxis + 1) % 3;
        int axisB = (depthAxis + 2) % 3;
        int horizontalAxis = extents[axisA] >= extents[axisB] ? axisA : axisB;
        int verticalAxis = horizontalAxis == axisA ? axisB : axisA;

        Vector2[] centers =
        {
            new Vector2(-0.65f, 0.42f),
            new Vector2(-0.22f, 0.62f),
            new Vector2(0.22f, 0.62f),
            new Vector2(0.65f, 0.42f),
            new Vector2(0f, -0.3f)
        };
        Vector2[] radii =
        {
            new Vector2(0.2f, 0.25f),
            new Vector2(0.21f, 0.25f),
            new Vector2(0.21f, 0.25f),
            new Vector2(0.2f, 0.25f),
            new Vector2(0.46f, 0.4f)
        };

        for (int i = 0; i < paths.Length; i++)
            paths[i] = JaggedPawPadPath(
                centers[i], radii[i], horizontalAxis, verticalAxis, depthAxis, i);

        return paths;
    }

    private Vector3[] JaggedPawPadPath(
        Vector2 center, Vector2 radius, int horizontalAxis, int verticalAxis, int depthAxis, int padIndex)
    {
        Vector3[] points = new Vector3[ringSegments + 1];
        Vector3 extents = localBounds.extents;
        Vector3 localCenter = localBounds.center;
        localCenter[depthAxis] -= extents[depthAxis] * 0.55f;
        float motionAmplitude = Mathf.Min(extents[horizontalAxis], extents[verticalAxis]) * 0.025f;
        float motionSpeed = flowSpeed > 0.01f ? flowSpeed : 2f;

        for (int i = 0; i < ringSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / ringSegments;
            float motion = Mathf.Sin(angle * 3f + Time.time * motionSpeed + padIndex) * motionAmplitude;
            Vector3 localPoint = localCenter;
            localPoint[horizontalAxis] += (center.x + Mathf.Cos(angle) * radius.x) * extents[horizontalAxis]
                + Mathf.Cos(angle) * motion;
            localPoint[verticalAxis] += (center.y + Mathf.Sin(angle) * radius.y) * extents[verticalAxis]
                + Mathf.Sin(angle) * motion;
            points[i] = abilityRenderer.transform.TransformPoint(localPoint);
        }

        points[ringSegments] = points[0];
        return points;
    }

    private void DrawPawOutline(Vector3[] points, ref int lineIndex, int padIndex)
    {
        LineRenderer line = lines[lineIndex++];
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.startWidth = coreWidth * 1.8f;
        line.endWidth = line.startWidth;

        Color color = activeEnergyColor;
        color.a = 0.82f + Mathf.Sin(Time.time * 3f + padIndex) * 0.12f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 1f) });
        line.colorGradient = gradient;
        line.enabled = true;
    }

    private void DrawPawSpark(Vector3[] points, ref int lineIndex)
    {
        LineRenderer line = lines[lineIndex++];
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.startWidth = coreWidth * 1.5f;
        line.endWidth = coreWidth * 0.1f;

        Color glowColor = Color.Lerp(activeEnergyColor, Color.white, 0.55f);
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(glowColor, 0f),
                new GradientColorKey(Color.white, 0.45f),
                new GradientColorKey(glowColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0.8f, 0.45f),
                new GradientAlphaKey(0f, 1f)
            });
        line.colorGradient = gradient;
        line.enabled = true;
    }

    private Vector3[] JaggedRingPath()
    {
        Vector3[] points = new Vector3[ringSegments + 1];
        Vector3 center = localBounds.center;
        Vector3 extents = localBounds.extents;
        int depthAxis = 0;
        if (extents.y < extents[depthAxis]) depthAxis = 1;
        if (extents.z < extents[depthAxis]) depthAxis = 2;
        int axisA = (depthAxis + 1) % 3;
        int axisB = (depthAxis + 2) % 3;
        float radiusA = extents[axisA] * ringRadius;
        float radiusB = extents[axisB] * ringRadius;
        Vector3 localDepth = Vector3.zero;
        localDepth[depthAxis] = 1f;

        for (int i = 0; i < ringSegments; i++)
        {
            float angle = 2f * Mathf.PI * i / ringSegments + Time.time * flowSpeed;
            float offsetA = Mathf.Cos(angle);
            float offsetB = Mathf.Sin(angle);
            Vector3 localPoint = center;
            localPoint[axisA] += offsetA * radiusA;
            localPoint[axisB] += offsetB * radiusB;
            localPoint[depthAxis] -= extents[depthAxis] * 0.55f;
            Vector3 point = abilityRenderer.transform.TransformPoint(localPoint);
            Vector3 localRadial = Vector3.zero;
            localRadial[axisA] = offsetA;
            localRadial[axisB] = offsetB;
            point += abilityRenderer.transform.TransformDirection(localRadial).normalized
                * Random.Range(-jaggedness, jaggedness);
            point += abilityRenderer.transform.TransformDirection(localDepth)
                * Random.Range(-jaggedness * 0.35f, jaggedness * 0.35f);
            points[i] = point;
        }

        points[ringSegments] = points[0];
        return points;
    }

    private void OnDestroy()
    {
        if (abilityRenderer != null && sourceRendererStateCaptured)
        {
            abilityRenderer.enabled = sourceRendererWasEnabled;
            abilityRenderer.SetPropertyBlock(originalSourceProperties);
        }

        foreach (LineRenderer lr in lines)
            if (lr != null) Destroy(lr.gameObject);
    }
}
