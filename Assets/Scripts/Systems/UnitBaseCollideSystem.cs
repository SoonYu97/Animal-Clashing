using System.Threading;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct UnitBaseCollideSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            state.GetComponentLookup<Unit>();
            state.GetComponentLookup<Base>();
        }
        
         [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var baseLookup = SystemAPI.GetComponentLookup<Base>();

            
            state.Dependency = new UnitMeetTriggerEventJob
            {
                UnitLookup = unitLookup,
                BaseLookup = baseLookup,
                CommandBuffer = ecb
            }.Schedule(simulation, state.Dependency);
            
        }

        [BurstCompile]
        private struct UnitMeetTriggerEventJob : ITriggerEventsJob
        {
            public ComponentLookup<Unit> UnitLookup;
            public ComponentLookup<Base> BaseLookup;
            public EntityCommandBuffer CommandBuffer;

            public void Execute(TriggerEvent triggerEvent)
            {
                if (!TryGetUnitBase(triggerEvent, out var unitEntity, out var baseEntity)) return;
                if (!AreDifferentPlayers(unitEntity, baseEntity)) return;
                
                // Add Score to unitEntity Tag
                var tagScore = UnitLookup.GetRefRO(unitEntity).ValueRO.Tag;
                CommandBuffer.DestroyEntity(unitEntity);
            }

            private bool TryGetUnitBase(TriggerEvent triggerEvent, out Entity unitEntity, out Entity baseEntity)
            {
                unitEntity = default;
                baseEntity = default;
                if (UnitLookup.HasComponent(triggerEvent.EntityA) && BaseLookup.HasComponent(triggerEvent.EntityB))
                {
                    unitEntity = triggerEvent.EntityA;
                    baseEntity = triggerEvent.EntityB;
                    return true;
                }
                if (UnitLookup.HasComponent(triggerEvent.EntityB) && BaseLookup.HasComponent(triggerEvent.EntityA))
                {
                    unitEntity = triggerEvent.EntityB;
                    baseEntity = triggerEvent.EntityA;
                    return true;
                }
                return false;
            }

            private bool AreDifferentPlayers(Entity unitEntity, Entity baseEntity)
            {
                var unitTag = UnitLookup[unitEntity];
                var baseTag = BaseLookup[baseEntity];
                
                return unitTag.Tag != baseTag.Tag;
            }
        }
    }
}