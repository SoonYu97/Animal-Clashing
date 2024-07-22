using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerDataAuthoring : MonoBehaviour
    {
        public PlayerTag Tag;
        public float Lives = 100f;

        public class Baker : Baker<PlayerDataAuthoring>
        {
            public override void Bake(PlayerDataAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var unitTypes = Enum.GetValues(typeof(UnitType));
                var multipliers = new List<AttributeMultiplier>(unitTypes.Length);
                multipliers.AddRange(from object unitType in unitTypes
                    select new AttributeMultiplier
                    {
                        UnitType = (UnitType) unitType,
                        UnitHealthModifier = 1,
                        UnitStrengthModifier = 1,
                        UnitSpeedModifier = 1,
                        UnitAttackRateModifier = 1,
                        UnitAttackRangeModifier = 1
                    });
                AddComponent(entity, new PlayerData
                {
                    Tag = authoring.Tag,
                    Lives = authoring.Lives,
                    KillCount = 0,
                    InitialLives = authoring.Lives,
                    InitialKillCount = 0
                });
                var attributeMultipliers = AddBuffer<AttributeMultiplier>(entity);
                attributeMultipliers.CopyFrom(multipliers.ToArray());
            }
        }
    }

    public struct PlayerData : IComponentData
    {
        public PlayerTag Tag;
        public FixedList32Bytes<UnitType> UnitQueue;
        public float Lives;
        public int KillCount;
        public float InitialLives;
        public int InitialKillCount;
    }

    [Serializable]
    public struct AttributeMultiplier : IBufferElementData
    {
        public UnitType UnitType;
        public float UnitHealthModifier;
        public float UnitStrengthModifier;
        public float UnitSpeedModifier;
        public float UnitAttackRateModifier;
        public float UnitAttackRangeModifier;
    }
}