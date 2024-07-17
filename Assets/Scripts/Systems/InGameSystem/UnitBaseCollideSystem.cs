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
            if (!IsGamePlaying(ref state)) return;

            var ecb = CreateCommandBuffer(ref state);
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var baseLookup = SystemAPI.GetComponentLookup<Base>();

            var scoringSystem = GetScoringSystem(ref state);
            var eventQueueParallel = eventQueue.AsParallelWriter();

            state.Dependency = new UnitBaseTriggerEventJob
            {
                UnitLookup = unitLookup,
                BaseLookup = baseLookup,
                CommandBuffer = ecb,
                EventQueueParallel = eventQueueParallel
            }.Schedule(simulation, state.Dependency);
            ProcessEventQueue(ref scoringSystem, ref state);
        }

        private bool IsGamePlaying(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            return gameState.Value == GameState.State.Playing;
        }

        private EntityCommandBuffer CreateCommandBuffer(ref SystemState state)
        {
            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        }

        private ref ScoringSystem GetScoringSystem(ref SystemState state)
        {
            var systemHandle = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ScoringSystem>();
            return ref state.WorldUnmanaged.GetUnsafeSystemRef<ScoringSystem>(systemHandle);
        }

        private void ProcessEventQueue(ref ScoringSystem scoringSystem, ref SystemState state)
        {
            state.Dependency.Complete();
            while (eventQueue.TryDequeue(out var unitTouchBaseEvent))
                scoringSystem.ReduceLives(unitTouchBaseEvent.Tag, unitTouchBaseEvent.Damage, ref state);
        }
    }
}