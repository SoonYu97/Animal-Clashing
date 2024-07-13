using Unity.Entities;

namespace DefaultNamespace
{
    public struct PlayerData : IComponentData
    {
        public PlayerTag Tag;
        public float Lives;
        public float UnitHealthModifier;
        public float UnitStrengthModifier;
        public float UnitSpeedModifier;
        public float UnitAttackRateModifier;
    }
}