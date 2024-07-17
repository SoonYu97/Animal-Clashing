using UnitInteraction;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace DefaultNamespace
{
    [BurstCompile]
    public partial struct UnitUnitCollideSystem : ISystem
    {
        private NativeQueue<UnitKilledEvent> eventQueue;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameState>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SimulationSingleton>();

            eventQueue = new NativeQueue<UnitKilledEvent>(Allocator.Persistent);
        }

        public void OnDestroy(ref SystemState state)
        {
            eventQueue.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!IsGamePlaying(ref state)) return;

            var currentTime = SystemAPI.Time.ElapsedTime;
            var ecb = CreateCommandBuffer(ref state);
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>();
            var unitLookup = SystemAPI.GetComponentLookup<Unit>();
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>();

            var scoringSystem = GetScoringSystem(ref state);
            var eventQueueParallel = eventQueue.AsParallelWriter();

            state.Dependency = new UnitMeetTriggerEventJob
            {
                UnitLookup = unitLookup,
                LocalTransformLookup = localTransformLookup,
                CurrentTime = currentTime,
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
            while (eventQueue.TryDequeue(out var unitKilledEvent))
                scoringSystem.IncreaseScore(unitKilledEvent.Tag, unitKilledEvent.Score, ref state);
        }
    }
}