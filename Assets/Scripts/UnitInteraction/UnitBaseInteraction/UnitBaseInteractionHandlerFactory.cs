using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace UnitInteraction
{
    public static class UnitBaseInteractionHandlerFactory
    {
        public static IUnitBaseInteractionHandler CreateHandler(TriggerEvent triggerEvent,
            ComponentLookup<Unit> unitLookup,
            ComponentLookup<Base> baseLookup, EntityCommandBuffer commandBuffer,
            NativeQueue<UnitTouchBaseEvent>.ParallelWriter eventQueueParallel)
        {
            if (IsDifferentPlayers(triggerEvent, unitLookup, baseLookup))
                return new DifferentPlayerBaseInteractionHandler(unitLookup, baseLookup, commandBuffer,
                    eventQueueParallel);

            return null;
        }

        private static bool IsDifferentPlayers(TriggerEvent triggerEvent, ComponentLookup<Unit> unitLookup,
            ComponentLookup<Base> baseLookup)
        {
            Unit unitTag;
            Base baseTag;
            if (unitLookup.HasComponent(triggerEvent.EntityA) && baseLookup.HasComponent(triggerEvent.EntityB))
            {
                unitTag = unitLookup[triggerEvent.EntityA];
                baseTag = baseLookup[triggerEvent.EntityB];
                return unitTag.Tag != baseTag.Tag;
            }

            if (unitLookup.HasComponent(triggerEvent.EntityB) && baseLookup.HasComponent(triggerEvent.EntityA))
            {
                unitTag = unitLookup[triggerEvent.EntityB];
                baseTag = baseLookup[triggerEvent.EntityA];
                return unitTag.Tag != baseTag.Tag;
            }

            return false;
        }
    }
}