using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

namespace DefaultNamespace
{	
    [BurstCompile]
    public partial class UnitQueueSystem : SystemBase
    {
        private double nextUpdateTime;
        
        private UnitQueue unitQueue;
        private LaneConfig laneConfig;
        private DynamicBuffer<UnitTypes> unitTypes;

        protected override void OnCreate()
        {
            RequireForUpdate<UnitQueue>();
            RequireForUpdate<LaneConfig>();
            RequireForUpdate<UnitTypes>();
        }

        protected override void OnUpdate()
        {
            unitQueue = SystemAPI.GetSingleton<UnitQueue>();
            laneConfig = SystemAPI.GetSingleton<LaneConfig>();
            unitTypes = SystemAPI.GetSingletonBuffer<UnitTypes>();

            if (nextUpdateTime == 0)
                nextUpdateTime = SystemAPI.Time.ElapsedTime + laneConfig.UnitSpawnStartDelay;
            
            var currentTime = SystemAPI.Time.ElapsedTime;
            if (currentTime < nextUpdateTime)
                return;
            nextUpdateTime = currentTime + laneConfig.UnitSpawnInterval;

            var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 100000));
            AddUnitToQueue(ref unitQueue.Queue1, random.NextInt(0, unitTypes.Length));
            AddUnitToQueue(ref unitQueue.Queue2, random.NextInt(0, unitTypes.Length));
            SystemAPI.SetSingleton(unitQueue);
        }

        private void AddUnitToQueue(ref FixedList32Bytes<int> queue, Unity.Mathematics.Random random)
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