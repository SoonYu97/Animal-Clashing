using Unity.Entities;

namespace DefaultNamespace
{
    public class RangerAuthoring : BaseUnit
    {
        public class Baker : Baker<RangerAuthoring>
        {
            public override void Bake(RangerAuthoring authoring)
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