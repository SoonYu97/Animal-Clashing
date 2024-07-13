using Unity.Entities;

namespace DefaultNamespace
{
    public struct Unit : IComponentData
    {
        public PlayerTag Tag;
        public float Health;
        public float Strength;
        public float Speed;
        public int CanMove;
        public float AttackRate;
        public double LastAttackTime;
    }
}