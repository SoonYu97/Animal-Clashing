using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct ScoringSystem : ISystem
    {
        private RefRW<PlayerData> player1Data;
        private RefRW<PlayerData> player2Data;
        
        public void OnCreate(ref SystemState state)
        {
            state.GetComponentLookup<Unit>();
            state.GetComponentLookup<Base>();
            state.GetComponentLookup<PlayerData>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                if (playerData.ValueRO.Tag == PlayerTag.Player1)
                    player1Data = playerData;
                if (playerData.ValueRO.Tag == PlayerTag.Player2)
                    player2Data = playerData;
            }
        }
        
        public void ReduceLives(PlayerTag tag, float damage)
        {
            if (tag == PlayerTag.Player1)
                player1Data.ValueRW.Lives -= damage;
            if (tag == PlayerTag.Player2)
                player2Data.ValueRW.Lives -= damage;
        }
        
        public void IncreaseScore(PlayerTag tag, int score)
        {
            if (tag == PlayerTag.Player1)
                player1Data.ValueRW.KillCount += score;
            if (tag == PlayerTag.Player2)
                player2Data.ValueRW.KillCount += score;
        }
        
    }
}