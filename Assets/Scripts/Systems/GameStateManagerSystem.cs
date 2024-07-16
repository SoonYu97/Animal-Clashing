using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class GameStateManagerSystem : SystemBase
    {
        protected override void OnCreate()
        {
            var entityManager = EntityManager;

            entityManager.AddComponent<GameState>(SystemHandle);
            entityManager.SetComponentData(SystemHandle, new GameState { Value = GameState.State.Playing });
        }

        protected override void OnUpdate()
        {
            if (GetPlayerData(PlayerTag.Player1).ValueRO.Lives <= 0)
            {
                Debug.Log("Player 2 Wins");
                EndDame();
            }
            if (GetPlayerData(PlayerTag.Player2).ValueRO.Lives <= 0)
            {
                Debug.Log("Player 1 Wins");
                EndDame();
            }
        }

        public void StartGame()
        {
            EntityManager.SetComponentData(SystemHandle, new GameState { Value = GameState.State.Playing });
        }

        public void EndDame()
        {
            EntityManager.SetComponentData(SystemHandle, new GameState { Value = GameState.State.GameOver });
        }
        
        private RefRO<PlayerData> GetPlayerData(PlayerTag playerTag)
        {
            foreach (var playerData in SystemAPI.Query<RefRO<PlayerData>>())
            {
                if (playerData.ValueRO.Tag == playerTag)
                    return playerData;
            }
            return default;
        }
        
        private RefRW<PlayerData> GetPlayerDataRW(PlayerTag playerTag)
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