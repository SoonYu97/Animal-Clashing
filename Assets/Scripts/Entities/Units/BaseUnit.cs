using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class BaseUnit : MonoBehaviour
    {
        public float Health;
        public float Strength;
        public float Speed;
        public float AttackRate;
        public float AttackRange;
        
        

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}