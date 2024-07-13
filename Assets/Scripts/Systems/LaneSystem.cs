using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class LaneSystem : SystemBase
    {
        private RefRW<UnitQueue> unitQueue;
        private DynamicBuffer<UnitSpawnLocations> unitSpawnLocations;
        private DynamicBuffer<UnitTypes> unitTypes;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<UnitQueue>();
            RequireForUpdate<UnitSpawnLocations>();
            RequireForUpdate<UnitTypes>();
        }

        protected override void OnUpdate()
        {
            unitQueue = SystemAPI.GetSingletonRW<UnitQueue>();
            unitSpawnLocations = SystemAPI.GetSingletonBuffer<UnitSpawnLocations>();
            unitTypes = SystemAPI.GetSingletonBuffer<UnitTypes>();
        }
        
        public void SpawnUnitFor(PlayerTag playerTag, int lane)
        {
            if (!TryGetFirstUnitTypeFromQueue(playerTag, out var unitType)) return;
            var unitEntity = EntityManager.Instantiate(unitTypes[unitType].Unit);
            EntityManager.SetComponentData(unitEntity, LocalTransform.FromPosition(GetSpawnLocation(playerTag, lane)));
            var unit = SystemAPI.GetComponentRW<Unit>(unitEntity);
            unit.ValueRW.Tag = playerTag;
        }
        
        private bool TryGetFirstUnitTypeFromQueue(PlayerTag playerTag, out int unitType)
        {
            unitType = 0;
            var queue = GetQueueForPlayer(playerTag);
            if (queue.IsEmpty) return false;
            unitType = queue[0];
            RemoveUnitFromQueue(playerTag, 0);
            return true;
        }
        
        private FixedList32Bytes<int> GetQueueForPlayer(PlayerTag playerTag)
        {
            return playerTag == PlayerTag.Player1 ? unitQueue.ValueRO.Queue1 : unitQueue.ValueRO.Queue2;
        }
        
        private void RemoveUnitFromQueue(PlayerTag playerTag, int index)
        {
            if (playerTag == PlayerTag.Player1)
                unitQueue.ValueRW.Queue1.RemoveAt(index);
            else
                unitQueue.ValueRW.Queue2.RemoveAt(index);
        }
        
        private float3 GetSpawnLocation(PlayerTag playerTag, int lane)
        {
            var adjustedLane = playerTag == PlayerTag.Player1 ? lane - 1 : lane - 1 + unitSpawnLocations.Length / 2;
            return unitSpawnLocations[adjustedLane].SpawnLocation;
        }
    }
}