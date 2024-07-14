using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace UnitInteraction
{
    public struct UnitBaseTriggerEventJob : ITriggerEventsJob
    {
        public ComponentLookup<Unit> UnitLookup;
        public ComponentLookup<Base> BaseLookup;
        public EntityCommandBuffer CommandBuffer;
        public NativeQueue<UnitTouchBaseEvent>.ParallelWriter EventQueueParallel;

        public void Execute(TriggerEvent triggerEvent)
        {
            var handler = UnitBaseInteractionHandlerFactory.CreateHandler(triggerEvent, UnitLookup, BaseLookup, CommandBuffer, EventQueueParallel);
            handler?.HandleInteraction(triggerEvent);
        }
    }
}