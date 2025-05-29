using UnityEngine;
using System.Collections;
using Project.Runtime.VFX;

public class AfterImageFader : MonoBehaviour
{
    private Material material;
    private float initialAlpha;
    private Coroutine fadeCoroutine;
    private Pool afterImagePool;
    private Pool particlePool;

    public void StartFade(float lifetime, float fadeStart, Pool particlePool)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        this.particlePool = particlePool;

        material = GetComponent<MeshRenderer>().material;
        initialAlpha = material.GetFloat("_Alpha");

        fadeCoroutine = StartCoroutine(FadeOut(lifetime, fadeStart));
    }

    public void SetReturnPool(Pool pool)
    {
        afterImagePool = pool;
    }

    private IEnumerator FadeOut(float lifetime, float fadeStart)
    {
        float fadeDuration = lifetime - fadeStart;
        yield return new WaitForSeconds(fadeStart);

        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(initialAlpha, 0f, t / fadeDuration);
            material.SetFloat("_Alpha", alpha);
            t += Time.deltaTime;
            yield return null;
        }

        material.SetFloat("_Alpha", 0f);
        gameObject.SetActive(false);
        afterImagePool?.ReturnParticle(gameObject);
    }

    public void SpawnParticlesFromMesh(Mesh mesh, Matrix4x4 localToWorld, Color rimColor)
    {
        if (particlePool == null || mesh == null) return;

        GameObject particleGO = particlePool.GetParticle();
        if (particleGO == null) return;

        particleGO.transform.SetPositionAndRotation(
            localToWorld.MultiplyPoint(Vector3.zero), 
            Quaternion.LookRotation(localToWorld.GetColumn(2), localToWorld.GetColumn(1)));
        particleGO.transform.localScale = Vector3.one;

        if (!particleGO.TryGetComponent(out ParticleSystem ps)) return;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Mesh;
        shape.mesh = mesh;
        shape.meshShapeType = ParticleSystemMeshShapeType.Vertex;
        shape.normalOffset = 0f;

        var main = ps.main;
        main.startColor = rimColor;

        ps.Emit(mesh.vertexCount / 3);

        StartCoroutine(ReturnToPool(particleGO, main.startLifetime.constantMax));
    }

    private IEnumerator ReturnToPool(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        go.SetActive(false);
        particlePool?.ReturnParticle(go);
    }
}
