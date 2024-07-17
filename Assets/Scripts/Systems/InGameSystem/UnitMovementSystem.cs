using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DefaultNamespace
{
    [UpdateBefore(typeof(UnitUnitCollideSystem))]
    internal partial struct UnitResetMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!IsGamePlaying(ref state)) return;
            foreach (var unit in SystemAPI
                         .Query<RefRW<Unit>>())
                unit.ValueRW.CanMove = 1;
        }

        private bool IsGamePlaying(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            return gameState.Value == GameState.State.Playing;
        }
    }

    [UpdateAfter(typeof(UnitUnitCollideSystem))]
    internal partial struct UnitMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!IsGamePlaying(ref state)) return;
            foreach (var (localTransform, unit) in SystemAPI
                         .Query<RefRW<LocalTransform>, RefRO<Unit>>())
            {
                if (unit.ValueRO.CanMove == 0) continue;
                var position = localTransform.ValueRO.Position;
                var direction = unit.ValueRO.Tag == PlayerTag.Player1 ? 1 : -1;
                position.x += unit.ValueRO.Speed * SystemAPI.Time.DeltaTime * direction;
                localTransform.ValueRW.Position = position;
            }
        }

        private bool IsGamePlaying(ref SystemState state)
        {
            var gameState = SystemAPI.GetSingleton<GameState>();
            return gameState.Value == GameState.State.Playing;
        }
    }
}