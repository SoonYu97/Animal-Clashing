using Unity.Collections;
using Unity.Entities;

namespace DefaultNamespace
{
    public struct PlayerData : IComponentData
    {
        public PlayerTag Tag;
        public FixedList32Bytes<int> Queue;
        public float Lives;
        public float UnitHealthModifier;
        public float UnitStrengthModifier;
        public float UnitSpeedModifier;
        public float UnitAttackRateModifier;
        public int KillCount;
    }
}