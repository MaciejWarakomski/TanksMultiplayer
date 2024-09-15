using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    public class SpawnPoint : MonoBehaviour
    {
        private static readonly List<SpawnPoint> SpawnPoints = new();

        private void OnEnable()
        {
            SpawnPoints.Add(this);
        }

        private void OnDisable()
        {
            SpawnPoints.Remove(this);
        }
        
        public static Vector3 GetRandomSpawnPos()
        {
            return SpawnPoints.Count == 0 ? 
                Vector3.zero : 
                SpawnPoints[Random.Range(0, SpawnPoints.Count)].transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }
}