using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class UnitQueueSystem : SystemBase
    {
        private double nextUpdateTime;

        private LaneConfig laneConfig;
        private DynamicBuffer<UnitTypes> unitTypes;

        protected override void OnCreate()
        {
            RequireForUpdate<GameState>();
            RequireForUpdate<LaneConfig>();
            RequireForUpdate<UnitTypes>();
        }

        protected override void OnUpdate()
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            if (gameState.Value != GameState.State.Playing) return;

            laneConfig = SystemAPI.GetSingleton<LaneConfig>();
            unitTypes = SystemAPI.GetSingletonBuffer<UnitTypes>();

            if (nextUpdateTime == 0)
                nextUpdateTime = SystemAPI.Time.ElapsedTime + laneConfig.UnitSpawnStartDelay;

            var currentTime = SystemAPI.Time.ElapsedTime;
            if (currentTime < nextUpdateTime)
                return;
            nextUpdateTime = currentTime + laneConfig.UnitSpawnInterval;

            var random = new Random((uint)UnityEngine.Random.Range(1, 100000));

            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
                playerData.ValueRW.Queue.Add(random.NextInt(0, unitTypes.Length));
        }

        private void AddUnitToQueue(ref FixedList32Bytes<int> queue, Random random)
        {
            if (queue.Length >= laneConfig.MaximumUnitOnHand) return;
            var unitType = random.NextInt(0, unitTypes.Length);
            queue.Add(unitType);
        }

        private void AddUnitToQueue(ref FixedList32Bytes<int> queue, int unitType)
        {
            queue.Add(unitType);
        }
    }
}