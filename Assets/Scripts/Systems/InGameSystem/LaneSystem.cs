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
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<UnitSpawnLocations>();
            RequireForUpdate<UnitTypes>();
        }

        protected override void OnUpdate()
        {
        }
        
        public void SpawnUnitFor(PlayerTag playerTag, int lane)
        {
            var playerData = GetPlayerData(playerTag);
            var spawnLocation = GetSpawnLocation(playerTag, lane);
            var unitTypes = SystemAPI.GetSingletonBuffer<UnitTypes>();
            
            if (IsLocationOccupied(spawnLocation)) return;
            if (!TryGetFirstUnitTypeFromQueue(playerData, out var unitType)) return;
            
            var unitEntity = EntityManager.Instantiate(unitTypes[unitType].Unit);
            
            EntityManager.SetComponentData(unitEntity, LocalTransform.FromPosition(spawnLocation));
            
            var unit = SystemAPI.GetComponentRW<Unit>(unitEntity);
            unit.ValueRW.Tag = playerTag;
        }

        private RefRW<PlayerData> GetPlayerData(PlayerTag playerTag)
        {
            foreach (var playerData in SystemAPI.Query<RefRW<PlayerData>>())
            {
                if (playerData.ValueRO.Tag == playerTag)
                    return playerData;
            }
            return default;
        }
        
        private bool TryGetFirstUnitTypeFromQueue(RefRW<PlayerData> playerData, out int unitType)
        {
            unitType = 0;
            var queue = playerData.ValueRO.Queue;
            if (queue.IsEmpty) return false;
            unitType = queue[0];
            playerData.ValueRW.Queue.RemoveAt(0);
            return true;
        }
        
        private float3 GetSpawnLocation(PlayerTag playerTag, int lane)
        {
            var unitSpawnLocations = SystemAPI.GetSingletonBuffer<UnitSpawnLocations>();
            var adjustedLane = playerTag == PlayerTag.Player1 ? lane - 1 : lane - 1 + unitSpawnLocations.Length / 2;
            return unitSpawnLocations[adjustedLane].SpawnLocation;
        }

        private bool IsLocationOccupied(float3 spawnLocation)
        {
            foreach (var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<Unit>())
            {
                if (!(math.distance(localTransform.ValueRO.Position, spawnLocation) < 0.5f)) continue;
                return true;
            }
            return false;
        }
    }
}