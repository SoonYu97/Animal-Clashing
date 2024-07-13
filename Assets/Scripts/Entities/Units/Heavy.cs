using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class Heavy : MonoBehaviour
    {
        public float Health;
        public float Strength;
        public float Speed;
        public float AttackRate;
        
        public class Baker : Baker<Heavy>
        {
            public override void Bake(Heavy authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {
                    Health = authoring.Health,
                    Strength = authoring.Strength,
                    Speed = authoring.Speed,
                    AttackRate = authoring.AttackRate,
                    LastAttackTime = 0,
                    CanMove = 1
                });
            }
        }
    }
}