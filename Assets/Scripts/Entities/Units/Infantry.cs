using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class Infantry : BaseUnit
    {
        public class Baker : Baker<Infantry>
        {
            public override void Bake(Infantry authoring)
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