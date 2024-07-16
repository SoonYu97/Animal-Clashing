using UnitInteraction;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct UnitBaseCollideSystem : ISystem
    {
        private NativeQueue<UnitTouchBaseEvent> eventQueue;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameState>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();

            eventQueue = new NativeQueue<UnitTouchBaseEvent>(Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            eventQueue.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            if (gameState.Value != GameState.State.Playing) return;

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var baseLookup = SystemAPI.GetComponentLookup<Base>();

            var systemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ScoringSystem>();
            ref var scoringSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<ScoringSystem>(systemHandle);

            var eventQueueParallel = eventQueue.AsParallelWriter();

            state.Dependency = new UnitBaseTriggerEventJob
            {
                UnitLookup = unitLookup,
                BaseLookup = baseLookup,
                CommandBuffer = ecb,
                EventQueueParallel = eventQueueParallel
            }.Schedule(simulation, state.Dependency);

            state.Dependency.Complete();
            while (eventQueue.TryDequeue(out var unitTouchBaseEvent))
                scoringSystem.ReduceLives(unitTouchBaseEvent.Tag, unitTouchBaseEvent.Damage, ref state);
        }
    }
}