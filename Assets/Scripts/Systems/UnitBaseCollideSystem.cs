using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct UnitBaseCollideSystem : ISystem
    {
        private struct UnitTouchBaseEvent
        {
            public float Damage;
            public PlayerTag Tag;
        }

        private NativeQueue<UnitTouchBaseEvent> eventQueue;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            state.GetComponentLookup<Unit>();
            state.GetComponentLookup<Base>();
            
            eventQueue = new NativeQueue<UnitTouchBaseEvent>(Allocator.Persistent);
        }
                
        public void OnDestroy(ref SystemState state)
        {
            eventQueue.Dispose();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var baseLookup = SystemAPI.GetComponentLookup<Base>();
            
            var systemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ScoringSystem>();
            ref var scoringSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<ScoringSystem>(systemHandle);

            var eventQueueParallel = eventQueue.AsParallelWriter();
            
            state.Dependency = new UnitMeetTriggerEventJob
            {
                UnitLookup = unitLookup,
                BaseLookup = baseLookup,
                CommandBuffer = ecb,
                EventQueueParallel = eventQueueParallel
            }.Schedule(simulation, state.Dependency);
            
            state.Dependency.Complete();
            while (eventQueue.TryDequeue(out var unitTouchBaseEvent))
            {
                scoringSystem.ReduceLives(unitTouchBaseEvent.Tag, unitTouchBaseEvent.Damage);
            }
        }

        [BurstCompile]
        private struct UnitMeetTriggerEventJob : ITriggerEventsJob
        {
            public ComponentLookup<Unit> UnitLookup;
            public ComponentLookup<Base> BaseLookup;
            public EntityCommandBuffer CommandBuffer;
            public NativeQueue<UnitTouchBaseEvent>.ParallelWriter EventQueueParallel;
            public void Execute(TriggerEvent triggerEvent)
            {
                if (!TryGetUnitBase(triggerEvent, out var unitEntity, out var baseEntity)) return;
                if (!AreDifferentPlayers(unitEntity, baseEntity)) return;
                
                // Add Score to unitEntity Tag
                var unitTag = UnitLookup.GetRefRO(unitEntity).ValueRO;
                var baseTag = BaseLookup.GetRefRO(baseEntity).ValueRO;
                
                EventQueueParallel.Enqueue(new UnitTouchBaseEvent
                {
                    Damage = unitTag.Health,
                    Tag = baseTag.Tag
                });
                
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