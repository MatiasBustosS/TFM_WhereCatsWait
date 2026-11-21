using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class OrbVfxPrefabSetup
{
    private const string PrefabPath = "Assets/Prefabs/OrbCollector.prefab";
    private const string MaterialFolder = "Assets/Materials&Shaders/OrbVfx";
    private const string TrailMaterialPath = MaterialFolder + "/M_OrbTrailVfx.mat";
    private const string ParticleMaterialPath = MaterialFolder + "/M_OrbParticlesVfx.mat";

    [MenuItem("Tools/Where Cats Wait/Configure Orb VFX")]
    public static void Apply()
    {
        EnsureMaterialFolder();
        Material trailMaterial = CreateMaterial(TrailMaterialPath, "Orb Trail");
        Material particleMaterial = CreateMaterial(ParticleMaterialPath, "Orb Particles");

        GameObject prefabContents = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Transform root = prefabContents.transform;
            GetOrCreateTrail(root, trailMaterial);
            GameObject particlesObject = GetOrCreateParticlesObject(root);
            ParticleSystem particles = ConfigureParticles(particlesObject.transform, particleMaterial);
            EnergyOrbVfxController vfxController = prefabContents.GetComponent<EnergyOrbVfxController>();
            if (vfxController == null)
            {
                vfxController = prefabContents.AddComponent<EnergyOrbVfxController>();
            }

            SerializedObject vfxObject = new SerializedObject(vfxController);
            vfxObject.FindProperty("orbParticles").objectReferenceValue = particles;
            TrailRenderer trail = root.Find("OrbTrail").GetComponent<TrailRenderer>();
            vfxObject.FindProperty("orbTrail").objectReferenceValue = trail;
            vfxObject.ApplyModifiedPropertiesWithoutUndo();

            EnergyOrbController energyController = prefabContents.GetComponent<EnergyOrbController>();
            if (energyController == null)
            {
                throw new InvalidOperationException("OrbCollector prefab has no EnergyOrbController.");
            }

            SerializedObject energyObject = new SerializedObject(energyController);
            energyObject.FindProperty("orbParticles").objectReferenceValue = particlesObject;
            energyObject.FindProperty("orbVfxController").objectReferenceValue = vfxController;
            energyObject.FindProperty("emptyColor").colorValue = new Color(0.82f, 0.93f, 1f, 1f);
            energyObject.FindProperty("emptyScale").floatValue = 0.12f;
            energyObject.FindProperty("maxEnergyColor").colorValue = new Color(0.06f, 0.2f, 0.5f, 1f);
            energyObject.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefabContents, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Configured OrbCollector with URP trail and energy-responsive particles.");
    }

    private static void EnsureMaterialFolder()
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
        {
            AssetDatabase.CreateFolder("Assets/Materials&Shaders", "OrbVfx");
        }
    }

    private static Material CreateMaterial(string path, string materialName)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                throw new InvalidOperationException("Could not find the URP Particles/Unlit shader.");
            }

            material = new Material(shader)
            {
                name = materialName,
                renderQueue = (int)RenderQueue.Transparent
            };
            SetMaterialColor(material, "_BaseColor", Color.white);
            SetMaterialColor(material, "_Color", Color.white);
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }
            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            AssetDatabase.CreateAsset(material, path);
        }

        return material;
    }

    private static void SetMaterialColor(Material material, string propertyName, Color color)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, color);
        }
    }

    private static TrailRenderer GetOrCreateTrail(Transform root, Material material)
    {
        Transform trailTransform = root.Find("OrbTrail");
        if (trailTransform == null)
        {
            GameObject trailObject = new GameObject("OrbTrail");
            trailTransform = trailObject.transform;
            trailTransform.SetParent(root, false);
        }

        TrailRenderer trail = trailTransform.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = trailTransform.gameObject.AddComponent<TrailRenderer>();
        }

        trail.time = 0.45f;
        trail.minVertexDistance = 0.08f;
        trail.widthMultiplier = 0.12f;
        trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.09375f));
        trail.colorGradient = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.82f, 0.93f, 1f), 0f),
                new GradientColorKey(new Color(0.82f, 0.93f, 1f), 1f)
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        };
        trail.shadowCastingMode = ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.textureMode = LineTextureMode.Stretch;
        trail.alignment = LineAlignment.View;
        trail.numCornerVertices = 4;
        trail.numCapVertices = 4;
        trail.material = material;
        return trail;
    }

    private static GameObject GetOrCreateParticlesObject(Transform root)
    {
        Transform particlesTransform = root.Find("OrbParticles");
        if (particlesTransform == null)
        {
            particlesTransform = root.Find("Effects");
            if (particlesTransform != null)
            {
                particlesTransform.name = "OrbParticles";
            }
            else
            {
                GameObject particlesObject = new GameObject("OrbParticles");
                particlesTransform = particlesObject.transform;
                particlesTransform.SetParent(root, false);
            }
        }

        particlesTransform.gameObject.SetActive(true);
        return particlesTransform.gameObject;
    }

    private static ParticleSystem ConfigureParticles(Transform particlesRoot, Material material)
    {
        ParticleSystem particles = particlesRoot.GetComponentInChildren<ParticleSystem>(true);
        if (particles == null)
        {
            particles = particlesRoot.gameObject.AddComponent<ParticleSystem>();
        }

        ParticleSystem.MainModule main = particles.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.06f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.maxParticles = 100;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 8f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.72f, 0.9f, 1f), 0f),
                new GradientColorKey(new Color(0.45f, 0.78f, 1f), 0.5f),
                new GradientColorKey(new Color(0.45f, 0.78f, 1f), 1f)
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.45f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        });

        ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.55f),
            new Keyframe(0.25f, 1f),
            new Keyframe(1f, 0f)));

        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.material = material;
        return particles;
    }
}
