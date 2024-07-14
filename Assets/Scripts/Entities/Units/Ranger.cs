using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class Ranger : BaseUnit
    {
        public class Baker : Baker<Ranger>
        {
            public override void Bake(Ranger authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {
                    Health = authoring.Health,
                    Strength = authoring.Strength,
                    Speed = authoring.Speed,
                    AttackRate = authoring.AttackRate,
                    AttackRange = authoring.AttackRange,
                    LastAttackTime = 0,
                    CanMove = 1
                });
            }
        }
    }
}