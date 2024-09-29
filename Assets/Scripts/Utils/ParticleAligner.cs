using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleAligner : MonoBehaviour
    {
        private ParticleSystem.MainModule _psMain;
        
        private void Start()
        {
            _psMain = GetComponent<ParticleSystem>().main;
        }

        private void Update()
        {
            _psMain.startRotation = -transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        }
    }
}