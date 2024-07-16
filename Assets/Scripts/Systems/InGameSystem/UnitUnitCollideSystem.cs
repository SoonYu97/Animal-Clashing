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
            var gameState = SystemAPI.GetSingleton<GameState>();
            if (gameState.Value != GameState.State.Playing) return;

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
                scoringSystem.IncreaseScore(unitKilledEvent.Tag, unitKilledEvent.Score, ref state);
        }
    }
}