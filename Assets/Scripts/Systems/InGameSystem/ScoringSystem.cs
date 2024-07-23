using Unity.Burst;
using Unity.Entities;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct ScoringSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
        }

        public void ReduceLives(PlayerTag tag, float damage, ref SystemState state)
        {
            GetPlayerData(tag, ref state).ValueRW.Lives -= damage;
        }

        public void IncreaseScore(PlayerTag tag, int score, ref SystemState state)
        {
            GetPlayerData(tag, ref state).ValueRW.TotalKillCount += score;
            GetPlayerData(tag, ref state).ValueRW.CurrentKillCount += score;
        }

        private RefRW<PlayerData> GetPlayerData(PlayerTag playerTag, ref SystemState state)
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                if (playerData.ValueRO.Tag == playerTag)
                    return playerData;
            }
            return default;
        }
    }
}