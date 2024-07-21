using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class UnitQueueSystem : SystemBase
    {
        private double nextUpdateTime;

        protected override void OnCreate()
        {
            RequireForUpdate<GameState>();
            RequireForUpdate<LaneConfig>();
            RequireForUpdate<UnitTypes>();
        }

        protected override void OnUpdate()
        {
            if (!IsGamePlaying()) return;

            var laneConfig = SystemAPI.GetSingleton<LaneConfig>();
            var unitTypes = SystemAPI.GetSingletonBuffer<UnitTypes>();

            if (!IsTimeToUpdate(laneConfig)) return;
            ScheduleNextUpdate(laneConfig);

            var random = CreateRandom();
            UpdatePlayerQueues(unitTypes, random);
        }

        private bool IsGamePlaying()
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            return gameState.Value == GameState.State.Playing;
        }

        private bool IsTimeToUpdate(LaneConfig laneConfig)
        {
            var currentTime = SystemAPI.Time.ElapsedTime;
            if (nextUpdateTime == 0)
            {
                nextUpdateTime = currentTime + laneConfig.UnitSpawnStartDelay;
                return false;
            }
            return currentTime >= nextUpdateTime;
        }

        private void ScheduleNextUpdate(LaneConfig laneConfig)
        {
            var currentTime = SystemAPI.Time.ElapsedTime;
            nextUpdateTime = currentTime + laneConfig.UnitSpawnInterval;
        }

        private Random CreateRandom()
        {
            return new Random((uint)UnityEngine.Random.Range(1, 100000));
        }

        private void UpdatePlayerQueues(DynamicBuffer<UnitTypes> unitTypes, Random random)
        {
            var totalWeight = 0;
            foreach (var unitType in unitTypes)
            {
                totalWeight += unitType.SpawnWeight;
            }

            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                var randomValue  = random.NextInt(0, totalWeight);
                var cumulativeWeight = 0;
                for (var i = 0; i < unitTypes.Length; i++)
                {
                    cumulativeWeight += unitTypes[i].SpawnWeight;
                    if (randomValue >= cumulativeWeight) continue;
                    AddUnitToQueue(ref playerData.ValueRW.UnitQueue, (UnitType) i);
                    break;
                }
            }
        }

        private void AddUnitToQueue(ref FixedList32Bytes<UnitType> queue, UnitType unitType)
        {
            var laneConfig = SystemAPI.GetSingleton<LaneConfig>();
            if (queue.Length >= laneConfig.MaximumUnitOnHand) return;
            queue.Add(unitType);
        }
    }
}