using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class PlayerDataAuthoring : MonoBehaviour
    {
        public PlayerTag Tag;
        public float Lives = 100f;
        public float UnitHealthModifier = 1f;
        public float UnitStrengthModifier = 1f;
        public float UnitSpeedModifier = 1f;
        public float UnitAttackRateModifier = 1f;
        
        public class Baker : Baker<PlayerDataAuthoring>
        {
            public override void Bake(PlayerDataAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlayerData
                {
                    Tag = authoring.Tag,
                    Lives = authoring.Lives,
                    UnitHealthModifier = authoring.UnitHealthModifier,
                    UnitStrengthModifier = authoring.UnitStrengthModifier,
                    UnitSpeedModifier = authoring.UnitSpeedModifier,
                    UnitAttackRateModifier = authoring.UnitAttackRateModifier
                });
            }
        }
    }
}