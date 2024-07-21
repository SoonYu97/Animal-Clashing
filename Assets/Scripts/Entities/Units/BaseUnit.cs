using UnityEngine;

namespace DefaultNamespace
{
    public abstract class BaseUnit : MonoBehaviour
    {
        public UnitType UnitType;
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