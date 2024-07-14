using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial class LaneSystem : SystemBase
    {
        private RefRW<PlayerData> player1Data;
        private RefRW<PlayerData> player2Data;
        private DynamicBuffer<UnitSpawnLocations> unitSpawnLocations;
        private DynamicBuffer<UnitTypes> unitTypes;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<UnitSpawnLocations>();
            RequireForUpdate<UnitTypes>();
        }

        protected override void OnUpdate()
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                if (playerData.ValueRO.Tag == PlayerTag.Player1)
                    player1Data = playerData;
                if (playerData.ValueRO.Tag == PlayerTag.Player2)
                    player2Data = playerData;
            }
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
            return playerTag.Equals(PlayerTag.Player1) ? player1Data.ValueRO.Queue : player2Data.ValueRO.Queue;
        }
        
        private void RemoveUnitFromQueue(PlayerTag playerTag, int index)
        {
            if (playerTag == PlayerTag.Player1)
                player1Data.ValueRW.Queue.RemoveAt(index);
            else
                player2Data.ValueRW.Queue.RemoveAt(index);
        }
        
        private float3 GetSpawnLocation(PlayerTag playerTag, int lane)
        {
            var adjustedLane = playerTag == PlayerTag.Player1 ? lane - 1 : lane - 1 + unitSpawnLocations.Length / 2;
            return unitSpawnLocations[adjustedLane].SpawnLocation;
        }
    }
}