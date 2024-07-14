using Unity.Entities;

namespace DefaultNamespace
{
    public class HeavyAuthoring : BaseUnit
    {
        public class Baker : Baker<HeavyAuthoring>
        {
            public override void Bake(HeavyAuthoring authoring)
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