using Unity.Entities;

namespace DefaultNamespace
{
    public class InfantryAuthoring : BaseUnit
    {
        public class Baker : Baker<InfantryAuthoring>
        {
            public override void Bake(InfantryAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Unit
                {
                    UnitType = authoring.UnitType,
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