using UnityEngine;
using System.Collections.Generic;
using Project.Runtime.VFX;

public class AfterImage : MonoBehaviour
{
    public Material afterImageMaterial;
    public float spawnDistance = 0.5f;
    public float afterImageLifetime = 1.0f;
    public float fadeOutStart = 0.5f;

    [Header("Pool References")]
    public Pool afterImagePool;
    public Pool particlePool;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) >= spawnDistance)
        {
            CreateAfterImage();
            lastPosition = transform.position;
        }
    }

    void CreateAfterImage()
    {
        if (afterImagePool == null) return;

        SkinnedMeshRenderer[] skinnedMeshes = GetComponentsInChildren<SkinnedMeshRenderer>();

        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (SkinnedMeshRenderer smr in skinnedMeshes)
        {
            Mesh bakedMesh = new Mesh();
            smr.BakeMesh(bakedMesh);

            CombineInstance ci = new CombineInstance();
            ci.mesh = bakedMesh;
            Matrix4x4 matrix = smr.transform.localToWorldMatrix;

            Vector3 position = matrix.GetColumn(3);
            position.y = 0f;
            matrix.SetColumn(3, new Vector4(position.x, position.y, position.z, 1f));

            ci.transform = matrix;

            combineInstances.Add(ci);
        }

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        Bounds bounds = finalMesh.bounds;
        Vector3 offset = new Vector3(bounds.center.x, 0f, bounds.center.z);
        Vector3[] vertices = finalMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] -= offset;
        finalMesh.vertices = vertices;
        finalMesh.RecalculateBounds();

        GameObject afterImageGO = afterImagePool.GetParticle();
        if (afterImageGO == null) return;

        afterImageGO.transform.SetPositionAndRotation(transform.position, transform.rotation);
        afterImageGO.transform.localScale = transform.localScale;

        var mf = afterImageGO.GetComponent<MeshFilter>();
        var mr = afterImageGO.GetComponent<MeshRenderer>();
        var fader = afterImageGO.GetComponent<AfterImageFader>();

        mf.mesh = finalMesh;
        mr.material = new Material(afterImageMaterial);

        fader.SetReturnPool(afterImagePool);
        fader.StartFade(afterImageLifetime, fadeOutStart, particlePool);

        Color rimColor = afterImageMaterial.GetColor("_RimColor");
        fader.SpawnParticlesFromMesh(finalMesh, afterImageGO.transform.localToWorldMatrix, rimColor);

        afterImageGO.SetActive(true);
    }
}
