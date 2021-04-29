using System.Collections;
using UnityEngine;

namespace Player
{
    public class SnowballThrower : MonoBehaviour
    {
        private bool _isOnCooldown;
        
        [SerializeField]
        private Projectile snowballPrefab;

        [SerializeField]
        private GameObject snowballEmitter;

        [SerializeField]
        private float throwCooldown = 2;
        
        private IEnumerator CooldownCounter()
        {
            _isOnCooldown = true;
            yield return new WaitForSeconds(throwCooldown);
            _isOnCooldown = false;
        }

        public void Throw(Transform target)
        {
            if (_isOnCooldown) return;
            var instance = Instantiate(snowballPrefab, snowballEmitter.transform.position, target.rotation);
            instance.Fire();
            StartCoroutine(CooldownCounter());
        }
    }
}