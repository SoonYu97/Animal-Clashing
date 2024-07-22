using Unity.Burst;
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
            var spawnLocation = GetSpawnLocation(playerTag, lane);
            var unitTypes = SystemAPI.GetSingletonBuffer<UnitTypes>();
            
            if (IsLocationOccupied(spawnLocation)) return;
            if (!TryGetFirstUnitTypeFromQueue(playerTag, out var unitType)) return;
            
            var unitEntity = EntityManager.Instantiate(unitTypes[(int) unitType].Unit);
            
            EntityManager.SetComponentData(unitEntity, LocalTransform.FromPosition(spawnLocation));
            
            SetUnitAttributes(playerTag, unitEntity, unitType);
        }

        private void SetUnitAttributes(PlayerTag playerTag, Entity unitEntity, UnitType unitType)
        {
            var unit = SystemAPI.GetComponentRW<Unit>(unitEntity);
            var attributeMultipliers = GetPlayerUnitAttribute(playerTag);
            unit.ValueRW.Tag = playerTag;
            unit.ValueRW.UnitType = unitType;
            foreach (var attributeMultiplier in attributeMultipliers)
            {
                if (attributeMultiplier.UnitType != unitType)
                    continue;
                unit.ValueRW.Health = unit.ValueRO.Health * attributeMultiplier.UnitHealthModifier;
                unit.ValueRW.Strength = unit.ValueRO.Strength * attributeMultiplier.UnitStrengthModifier;
                unit.ValueRW.Speed = unit.ValueRO.Speed * attributeMultiplier.UnitSpeedModifier;
                unit.ValueRW.AttackRate = unit.ValueRO.AttackRate * attributeMultiplier.UnitAttackRateModifier;
                unit.ValueRW.AttackRange = unit.ValueRO.AttackRange * attributeMultiplier.UnitAttackRangeModifier;
                break;
            }
            
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

        private DynamicBuffer<AttributeMultiplier> GetPlayerUnitAttribute(PlayerTag playerTag)
        {
            foreach (var (playerData, entity) in SystemAPI.Query<RefRO<PlayerData>>().WithEntityAccess())
            {
                if (playerData.ValueRO.Tag == playerTag)
                    return SystemAPI.GetBuffer<AttributeMultiplier>(entity);
            }
            return default;
        }
        
        private bool TryGetFirstUnitTypeFromQueue(PlayerTag playerTag, out UnitType unitType)
        {
            unitType = 0;
            var playerData = GetPlayerDataRW(playerTag);
            var queue = playerData.ValueRO.UnitQueue;
            if (queue.IsEmpty) return false;
            unitType = queue[0];
            playerData.ValueRW.UnitQueue.RemoveAt(0);
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