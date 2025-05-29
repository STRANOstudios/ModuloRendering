using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Project.Runtime.VFX
{
    [HideMonoScript]
    public class Pool : MonoBehaviour
    {
        [BoxGroup("Settings")]
        [Tooltip("The prefab of the particle to pool.")]
        [SerializeField, AssetsOnly] 
        private GameObject m_particlePrefab;

        [BoxGroup("Settings")]
        [Tooltip("The number of particles to pool.")]
        [SerializeField, MinValue(1)]
        private int m_poolSize = 16;

        [ShowInInspector, ReadOnly]
        private readonly Queue<GameObject> m_pool = new Queue<GameObject>();

        #region Initialization and State Setup --------------------------

        private void Awake()
        {
            for (int i = 0; i < m_poolSize; i++)
            {
                GameObject obj = Instantiate(m_particlePrefab, transform);
                obj.SetActive(false);
                m_pool.Enqueue(obj);
            }
        }

        #endregion

        /// <summary>
        /// Retrieves a particle object from the pool.
        /// </summary>
        public GameObject GetParticle()
        {
            GameObject obj;

            if (m_pool.Count == 0)
            {
                obj = Instantiate(m_particlePrefab, transform);
            }
            else
            {
                obj = m_pool.Dequeue();
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Returns a particle object back to the pool.
        /// </summary>
        public void ReturnParticle(GameObject particle)
        {
            particle.SetActive(false);
            m_pool.Enqueue(particle);
        }
    }
}
