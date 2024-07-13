using System.Threading;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct UnitUnitCollideSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            state.GetComponentLookup<Unit>();
        }
        
         [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);


            var currentTime = SystemAPI.Time.ElapsedTime;
            
            state.Dependency = new UnitMeetTriggerEventJob
            {
                UnitLookup = unitLookup,
                CurrentTime = currentTime,
                CommandBuffer = ecb
            }.Schedule(simulation, state.Dependency);
            
        }

        [BurstCompile]
        private struct UnitMeetTriggerEventJob : ITriggerEventsJob
        {
            public ComponentLookup<Unit> UnitLookup;
            public EntityCommandBuffer CommandBuffer;

            public double CurrentTime;
            
            public void Execute(TriggerEvent triggerEvent)
            {
                if (!TryGetUnits(triggerEvent, out var unitEntityA, out var unitEntityB)) return;
                if (!AreDifferentPlayers(unitEntityA, unitEntityB)) return;
                
                StopEntity(unitEntityA);
                StopEntity(unitEntityB);
                
                AttackEntity(unitEntityA, unitEntityB);
                AttackEntity(unitEntityB, unitEntityA);
            }

            private bool TryGetUnits(TriggerEvent triggerEvent, out Entity entityA, out Entity entityB)
            {
                entityA = default;
                entityB = default;
                if (!UnitLookup.HasComponent(triggerEvent.EntityA) || !UnitLookup.HasComponent(triggerEvent.EntityB))
                    return false;
                entityA = triggerEvent.EntityA;
                entityB = triggerEvent.EntityB;
                return true;
            }

            private bool AreDifferentPlayers(Entity entityA, Entity entityB)
            {
                if (!UnitLookup.HasComponent(entityA) || !UnitLookup.HasComponent(entityB)) return false;

                var playerA = UnitLookup[entityA];
                var playerB = UnitLookup[entityB];
                
                return playerA.Tag != playerB.Tag;
            }

            private void StopEntity(Entity unitEntity)
            {
                var unit = UnitLookup.GetRefRW(unitEntity);
                Interlocked.Exchange(ref unit.ValueRW.CanMove, 0);
            }
            
            private void AttackEntity(Entity attacker, Entity defender)
            {
                var attackerUnit = UnitLookup.GetRefRW(attacker);
                var defenderUnit = UnitLookup.GetRefRW(defender);
                
                var timeBetweenAttacks = 60.0 / attackerUnit.ValueRO.AttackRate; // Convert rate to time between attacks in seconds

                if (!(CurrentTime - attackerUnit.ValueRO.LastAttackTime >= timeBetweenAttacks)) return;
                
                Interlocked.Exchange(ref defenderUnit.ValueRW.Health, defenderUnit.ValueRO.Health - attackerUnit.ValueRO.Strength);
                Interlocked.Exchange(ref attackerUnit.ValueRW.LastAttackTime, CurrentTime);
                
                DestroyEntityIfHealthLessThanZero(defender);
            }

            private void DestroyEntityIfHealthLessThanZero(Entity defender)
            {
                if (UnitLookup.GetRefRO(defender).ValueRO.Health <= 0)
                    CommandBuffer.DestroyEntity(defender);
            }
        }
    }
}