using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class GameStateManagerSystem : SystemBase
    {
        public event EventHandler<string> GameEnd;
        
        protected override void OnCreate()
        {
            var entityManager = EntityManager;

            entityManager.AddComponent<GameState>(SystemHandle);
            entityManager.SetComponentData(SystemHandle, new GameState { Value = GameState.State.Start });
        }

        protected override void OnUpdate()
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            if (gameState.Value != GameState.State.Playing) return;
            
            var player1Data = GetPlayerData(PlayerTag.Player1).ValueRO;
            var player2Data = GetPlayerData(PlayerTag.Player2).ValueRO;

            if (player1Data.Lives <= 0 && player2Data.Lives <= 0)
            {
                if (Mathf.Approximately(player1Data.Lives, player2Data.Lives))
                {
                    EndGame("Draw");
                }
                else if (player1Data.Lives < player2Data.Lives)
                {
                    EndGame("Player 2 Wins");
                }
                else
                {
                    EndGame("Player 1 Wins");
                }
                return;
            }
            if (player1Data.Lives <= 0)
            {
                EndGame("Player 2 Wins");
            }
            else if (player2Data.Lives <= 0)
            {
                EndGame("Player 1 Wins");
            }
        }

        public void StartGame()
        {
            EntityManager.SetComponentData(SystemHandle, new GameState { Value = GameState.State.Playing });
        }

        private void EndGame(string endGameString)
        {
            EntityManager.SetComponentData(SystemHandle, new GameState { Value = GameState.State.GameOver });
            GameEnd?.Invoke(this, endGameString);
            ResetGame();
        }

        private void ResetGame()
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                playerData.ValueRW.Lives = playerData.ValueRO.InitialLives;
                playerData.ValueRW.TotalKillCount = playerData.ValueRO.InitialKillCount;
                playerData.ValueRW.CurrentKillCount = playerData.ValueRO.InitialKillCount;
            }
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<Unit>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
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
    }
}