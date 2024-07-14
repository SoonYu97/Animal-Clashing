using DefaultNamespace;
using UnitInteraction.Interfaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace UnitInteraction
{
    public static class UnitInteractionHandlerFactory
    {
        public static IUnitInteractionHandler CreateHandler(
            Entity entityA, Entity entityB, ComponentLookup<Unit> unitLookup, 
            ComponentLookup<LocalTransform> localTransformLookup, EntityCommandBuffer commandBuffer,
            NativeQueue<UnitKilledEvent>.ParallelWriter eventQueueParallel, double currentTime)
        {
            if (!unitLookup.HasComponent(entityA) || !unitLookup.HasComponent(entityB)) return null;

            var playerA = unitLookup[entityA];
            var playerB = unitLookup[entityB];

            if (playerA.Tag != playerB.Tag)
            {
                return new DifferentPlayerInteractionHandler(unitLookup, localTransformLookup, commandBuffer, eventQueueParallel, currentTime);
            }

            return new SamePlayerInteractionHandler(unitLookup, localTransformLookup);
        }
    }
}