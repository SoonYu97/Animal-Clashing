using Unity.Collections;
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
                    KillCount = 0,
                    InitialLives = authoring.Lives,
                    InitialKillCount = 0,
                    UnitHealthModifier = authoring.UnitHealthModifier,
                    UnitStrengthModifier = authoring.UnitStrengthModifier,
                    UnitSpeedModifier = authoring.UnitSpeedModifier,
                    UnitAttackRateModifier = authoring.UnitAttackRateModifier
                });
            }
        }
    }
    
    public struct PlayerData : IComponentData
    {
        public PlayerTag Tag;
        public FixedList32Bytes<UnitType> UnitQueue;
        public float Lives;
        public float UnitHealthModifier;
        public float UnitStrengthModifier;
        public float UnitSpeedModifier;
        public float UnitAttackRateModifier;
        public int KillCount;
        public float InitialLives;
        public int InitialKillCount;
    }
}