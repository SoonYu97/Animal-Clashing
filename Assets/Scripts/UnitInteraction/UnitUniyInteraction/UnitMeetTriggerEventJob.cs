using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace UnitInteraction
{
    public struct UnitMeetTriggerEventJob : ITriggerEventsJob
    {
        public ComponentLookup<Unit> UnitLookup;
        public ComponentLookup<LocalTransform> LocalTransformLookup;
        public EntityCommandBuffer CommandBuffer;
        public NativeQueue<UnitKilledEvent>.ParallelWriter EventQueueParallel;
        public double CurrentTime;

        public void Execute(TriggerEvent triggerEvent)
        {
            var interactionHandler = UnitInteractionHandlerFactory.CreateHandler(
                triggerEvent.EntityA, triggerEvent.EntityB, 
                UnitLookup, LocalTransformLookup, CommandBuffer, 
                EventQueueParallel, CurrentTime);
            interactionHandler?.HandleInteraction(triggerEvent.EntityA, triggerEvent.EntityB);
        }
    }
}