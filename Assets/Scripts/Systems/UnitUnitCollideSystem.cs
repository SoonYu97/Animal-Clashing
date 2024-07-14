using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct UnitUnitCollideSystem : ISystem
    {
        private struct UnitKilledEvent
        {
            public int Score;
            public PlayerTag Tag;
        }

        private NativeQueue<UnitKilledEvent> eventQueue;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            state.GetComponentLookup<Unit>();
            state.GetComponentLookup<LocalTransform>();
            
            eventQueue = new NativeQueue<UnitKilledEvent>(Allocator.Persistent);
        }
                
        public void OnDestroy(ref SystemState state)
        {
            eventQueue.Dispose();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var currentTime = SystemAPI.Time.ElapsedTime;
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
            
            var systemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ScoringSystem>();
            ref var scoringSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<ScoringSystem>(systemHandle);
            
            var eventQueueParallel = eventQueue.AsParallelWriter();
            
            state.Dependency = new UnitMeetTriggerEventJob
            {
                UnitLookup = unitLookup,
                LocalTransformLookup = localTransformLookup,
                CurrentTime = currentTime,
                CommandBuffer = ecb,
                EventQueueParallel = eventQueueParallel
            }.Schedule(simulation, state.Dependency);
            
            state.Dependency.Complete();
            while (eventQueue.TryDequeue(out var unitKilledEvent))
            {
                scoringSystem.IncreaseScore(unitKilledEvent.Tag, unitKilledEvent.Score);
            }
        }

        [BurstCompile]
        private struct UnitMeetTriggerEventJob : ITriggerEventsJob
        {
            public ComponentLookup<Unit> UnitLookup;
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            public EntityCommandBuffer CommandBuffer;
            public NativeQueue<UnitKilledEvent>.ParallelWriter EventQueueParallel;

            public double CurrentTime;
            
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
                if (!(UnitLookup.GetRefRO(defender).ValueRO.Health <= 0)) return;
                CommandBuffer.DestroyEntity(defender);
                EventQueueParallel.Enqueue(new UnitKilledEvent
                {
                    Score = 1,
                    Tag = UnitLookup[defender].Tag
                });
            }
        }
    }
}