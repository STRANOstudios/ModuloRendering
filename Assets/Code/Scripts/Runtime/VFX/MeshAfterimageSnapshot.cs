using UnityEngine;

namespace Project.Runtime.VFX
{
    public class MeshAfterimageSnapshot : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Minimum distance before a new snapshot is taken")]
        [SerializeField] private float m_distanceThreshold = 1f;

        [Tooltip("Time before the afterimage fades out")]
        [SerializeField] private float m_fadeoutDuration = 1.5f;

        [Tooltip("Material to apply to the afterimage mesh")]
        [SerializeField] private Material m_afterimageMaterial;

        private Vector3 m_lastPosition;
        private float m_totalDistance;
        private SkinnedMeshRenderer[] m_renderers;

        #region Initialization and State Setup --------------------------

        private void Start()
        {
            m_lastPosition = transform.position;
            m_renderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        }

        #endregion

        private void Update()
        {
            Vector3 currentPosition = transform.position;
            m_totalDistance += Vector3.Distance(currentPosition, m_lastPosition);

            if (m_totalDistance >= m_distanceThreshold)
            {
                CreateAfterimage();
                m_totalDistance = 0f;
            }

            m_lastPosition = currentPosition;
        }

        #region Afterimage Creation -------------------------------------

        private void CreateAfterimage()
        {
            var allMeshes = new System.Collections.Generic.List<CombineInstance>();

            foreach (var smr in m_renderers)
            {
                if (smr == null || smr.sharedMesh == null)
                    continue;

                Mesh bakedMesh = new Mesh();
                smr.BakeMesh(bakedMesh);

                CombineInstance ci = new CombineInstance
                {
                    mesh = bakedMesh,
                    transform = smr.localToWorldMatrix
                };

                allMeshes.Add(ci);
            }

            if (allMeshes.Count == 0)
                return;

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(allMeshes.ToArray(), true, true);

            GameObject afterimage = new GameObject("Afterimage");
            afterimage.transform.SetPositionAndRotation(transform.position, transform.rotation);
            afterimage.transform.localScale = transform.lossyScale;

            MeshFilter mf = afterimage.AddComponent<MeshFilter>();
            mf.mesh = combinedMesh;

            MeshRenderer mr = afterimage.AddComponent<MeshRenderer>();
            if (m_afterimageMaterial != null)
            {
                mr.material = m_afterimageMaterial;
            }

            Destroy(afterimage, m_fadeoutDuration);
        }

        #endregion
    }
}
