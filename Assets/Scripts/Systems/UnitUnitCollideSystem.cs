using System.Threading;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

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
            state.GetComponentLookup<LocalTransform>();
        }
        
         [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTime = SystemAPI.Time.ElapsedTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            // foreach (var (unitA, localTransformA) in SystemAPI.Query<RefRW<Unit>, RefRO<LocalTransform>>())
            // {
            //     foreach (var (unitB, localTransformB, entityB) in SystemAPI.Query<RefRW<Unit>, RefRO<LocalTransform>>().WithEntityAccess())
            //     {
            //         if (unitA.ValueRO.Tag == unitB.ValueRO.Tag) continue;
            //         if (math.distancesq(localTransformA.ValueRO.Position.y, localTransformB.ValueRO.Position.y) >
            //             Epsilon) continue;
            //         // Debug.Log(unitA.ValueRO.Tag.ToString());
            //         var distance = math.distance(localTransformA.ValueRO.Position, localTransformB.ValueRO.Position);
            //         if (distance > unitA.ValueRO.AttackRange) continue;
            //
            //         unitA.ValueRW.CanMove = 0;
            //         
            //         var timeBetweenAttacks = 60.0 / unitA.ValueRO.AttackRate; // Convert rate to time between attacks in seconds
            //
            //         if (!(currentTime - unitA.ValueRO.LastAttackTime >= timeBetweenAttacks)) return;
            //
            //         unitB.ValueRW.Health = unitB.ValueRO.Health - unitA.ValueRO.Strength;
            //         unitA.ValueRW.LastAttackTime = currentTime;
            //         
            //         if (unitB.ValueRO.Health <= 0)
            //             ecb.DestroyEntity(entityB);
            //     }
            // }
            
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            
            state.Dependency = new UnitMeetTriggerEventJob
            {
                UnitLookup = unitLookup,
                LocalTransformLookup = localTransformLookup,
                CurrentTime = currentTime,
                CommandBuffer = ecb
            }.Schedule(simulation, state.Dependency);
            
        }

        [BurstCompile]
        private struct UnitMeetTriggerEventJob : ITriggerEventsJob
        {
            public ComponentLookup<Unit> UnitLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            public EntityCommandBuffer CommandBuffer;

            public double CurrentTime;
            
            private const float Epsilon = 0.001f;
            
            public void Execute(TriggerEvent triggerEvent)
            {
                if (!TryGetUnits(triggerEvent, out var unitEntityA, out var unitEntityB)) return;
                if (!AreDifferentPlayers(unitEntityA, unitEntityB)) return;

                StopAndAttackIfInRange(unitEntityA, unitEntityB);
                StopAndAttackIfInRange(unitEntityB, unitEntityA);
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

            private void StopAndAttackIfInRange(Entity attackerEntity, Entity defenderEntity)
            {
                if (!IsInRange(attackerEntity, defenderEntity)) return;
                StopEntity(attackerEntity);
                AttackEntity(attackerEntity, defenderEntity);
            }

            private bool IsInSameLane(Entity attacker, Entity defender)
            {
                return (LocalTransformLookup[attacker].Position.y - LocalTransformLookup[defender].Position.y) <= Epsilon;
            }

            private bool IsInRange(Entity attacker, Entity defender)
            {
                var distanceSq = math.distancesq(LocalTransformLookup[attacker].Position, LocalTransformLookup[defender].Position);
                return UnitLookup[attacker].AttackRange * UnitLookup[attacker].AttackRange > distanceSq;
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