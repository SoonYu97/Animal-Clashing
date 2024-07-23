using System;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class BoostSystem : SystemBase
    {
        private const int AttributeCount = 5;
        private const int KillCountPerBoost = 2;
        private int unitTypeCount;
        private (int, int) playerOneBoostSelection;
        private (int, int) playerTwoBoostSelection;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<UnitSpawnLocations>();
            RequireForUpdate<UnitTypes>();
            unitTypeCount = Enum.GetNames(typeof(UnitType)).Length;
        }

        protected override void OnUpdate()
        {
            if (!IsGamePlaying()) return;

            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));
            UpdateBoostSelections(random);
        }

        private bool IsGamePlaying()
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            return gameState.Value == GameState.State.Playing;
        }

        private void UpdateBoostSelections(Random random)
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                if (playerData.ValueRO.CurrentKillCount < KillCountPerBoost) continue;

                switch (playerData.ValueRO.Tag)
                {
                    case PlayerTag.Player1 when playerOneBoostSelection == default:
                        playerOneBoostSelection = GenerateBoostSelection(random);
                        break;
                    case PlayerTag.Player2 when playerTwoBoostSelection == default:
                        playerTwoBoostSelection = GenerateBoostSelection(random);
                        break;
                }
            }
        }

        public bool TryBoostPlayer1Unit(int selectionIndex)
        {
            return TryApplyBoost(ref playerOneBoostSelection, PlayerTag.Player1, selectionIndex);
        }

        public bool TryBoostPlayer2Unit(int selectionIndex)
        {
            return TryApplyBoost(ref playerTwoBoostSelection, PlayerTag.Player2, selectionIndex);
        }

        private bool TryApplyBoost(ref (int, int) boostSelection, PlayerTag playerTag, int selectionIndex)
        {
            if (boostSelection == default) return false;

            var boostIndex = selectionIndex == 0 ? boostSelection.Item1 : boostSelection.Item2;
            boostSelection = default;

            ApplyBoost(playerTag, boostIndex);
            return true;
        }

        private void ApplyBoost(PlayerTag playerTag, int boostIndex)
        {
            var unitType = boostIndex / AttributeCount;
            var attributeType = boostIndex % AttributeCount;
            var attributeMultipliers = GetUnitAttributes(playerTag);

            BoostUnitAttributes(attributeMultipliers, unitType, attributeType);
            DeductKillCount(playerTag);
        }

        private void BoostUnitAttributes(DynamicBuffer<AttributeMultiplier> attributeMultipliers, int unitType, int attributeType)
        {
            ref var unitAttributeMultiplier = ref attributeMultipliers.ElementAt(unitType);

            switch (attributeType)
            {
                case 0:
                    unitAttributeMultiplier.UnitHealthModifier *= 1.5f;
                    break;
                case 1:
                    unitAttributeMultiplier.UnitSpeedModifier *= 1.5f;
                    break;
                case 2:
                    unitAttributeMultiplier.UnitStrengthModifier *= 1.5f;
                    break;
                case 3:
                    unitAttributeMultiplier.UnitAttackRateModifier *= 1.5f;
                    break;
                case 4:
                    unitAttributeMultiplier.UnitAttackRangeModifier *= 1.5f;
                    break;
            }
        }

        private void DeductKillCount(PlayerTag playerTag)
        {
            var playerData = GetPlayerData(playerTag);
            playerData.ValueRW.CurrentKillCount -= KillCountPerBoost;
        }

        private (int, int) GenerateBoostSelection(Random random)
        {
            var firstIndex = random.NextInt(0, AttributeCount * unitTypeCount);
            int secondIndex;
            do
            {
                secondIndex = random.NextInt(0, AttributeCount * unitTypeCount);
            } while (secondIndex == firstIndex);

            return (firstIndex, secondIndex);
        }

        private RefRW<PlayerData> GetPlayerData(PlayerTag playerTag)
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                if (playerData.ValueRO.Tag == playerTag)
                    return playerData;
            }
            throw new InvalidOperationException($"PlayerData for {playerTag} not found.");
        }

        private DynamicBuffer<AttributeMultiplier> GetUnitAttributes(PlayerTag playerTag)
        {
            foreach (var (playerData, entity) in SystemAPI.Query<RefRO<PlayerData>>().WithEntityAccess())
            {
                if (playerData.ValueRO.Tag == playerTag)
                    return SystemAPI.GetBuffer<AttributeMultiplier>(entity);
            }
            throw new InvalidOperationException($"AttributeMultipliers for {playerTag} not found.");
        }
    }
}
