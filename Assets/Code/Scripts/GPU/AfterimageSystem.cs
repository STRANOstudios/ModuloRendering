using UnityEngine;
using System.Collections.Generic;

public class AfterimageSystem : MonoBehaviour
{
    [System.Serializable]
    public class MeshSource
    {
        public SkinnedMeshRenderer skinnedRenderer;
    }

    public MeshSource[] meshSources;
    public Material instancedMaterial;
    public float lifetime = 1.2f;

    struct InstanceData
    {
        public Mesh mesh;
        public Matrix4x4 matrix;
        public float spawnTime;
    }

    List<InstanceData> instances = new();

    void Update()
    {
        float now = Time.time;
        instances.RemoveAll(i => now - i.spawnTime > lifetime);

        foreach (var data in instances)
        {
            var props = new MaterialPropertyBlock();
            props.SetFloat("_SpawnTime", data.spawnTime);
            props.SetFloat("_LifeTime", lifetime);
            Graphics.DrawMesh(data.mesh, data.matrix, instancedMaterial, 0, null, 0, props);
        }
    }

    public void SpawnAfterImage(Transform rootTransform)
    {
        float now = Time.time;

        foreach (var source in meshSources)
        {
            if (source.skinnedRenderer == null) continue;

            var bakedMesh = new Mesh();
            source.skinnedRenderer.BakeMesh(bakedMesh);

            instances.Add(new InstanceData
            {
                mesh = bakedMesh,
                matrix = rootTransform.localToWorldMatrix,
                spawnTime = now
            });
        }
    }
}
