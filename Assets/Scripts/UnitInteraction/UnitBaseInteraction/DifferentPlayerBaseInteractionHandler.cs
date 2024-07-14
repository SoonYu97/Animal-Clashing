using DefaultNamespace;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace UnitInteraction
{
    public class DifferentPlayerBaseInteractionHandler : IUnitBaseInteractionHandler
{
    private ComponentLookup<Unit> unitLookup;
    private ComponentLookup<Base> baseLookup;
    private EntityCommandBuffer commandBuffer;
    private NativeQueue<UnitTouchBaseEvent>.ParallelWriter eventQueueParallel;

    public DifferentPlayerBaseInteractionHandler(ComponentLookup<Unit> unitLookup, ComponentLookup<Base> baseLookup,
                                                 EntityCommandBuffer commandBuffer, NativeQueue<UnitTouchBaseEvent>.ParallelWriter eventQueueParallel)
    {
        this.unitLookup = unitLookup;
        this.baseLookup = baseLookup;
        this.commandBuffer = commandBuffer;
        this.eventQueueParallel = eventQueueParallel;
    }

    public void HandleInteraction(TriggerEvent triggerEvent)
    {
        if (!TryGetUnitBase(triggerEvent, out var unitEntity, out var baseEntity)) return;
        if (!AreDifferentPlayers(unitEntity, baseEntity)) return;

        var unitTag = unitLookup[unitEntity];
        var baseTag = baseLookup[baseEntity];

        eventQueueParallel.Enqueue(new UnitTouchBaseEvent
        {
            Damage = unitTag.Health,
            Tag = baseTag.Tag
        });

        commandBuffer.DestroyEntity(unitEntity);
    }

    private bool TryGetUnitBase(TriggerEvent triggerEvent, out Entity unitEntity, out Entity baseEntity)
    {
        unitEntity = default;
        baseEntity = default;
        if (unitLookup.HasComponent(triggerEvent.EntityA) && baseLookup.HasComponent(triggerEvent.EntityB))
        {
            unitEntity = triggerEvent.EntityA;
            baseEntity = triggerEvent.EntityB;
            return true;
        }
        if (unitLookup.HasComponent(triggerEvent.EntityB) && baseLookup.HasComponent(triggerEvent.EntityA))
        {
            unitEntity = triggerEvent.EntityB;
            baseEntity = triggerEvent.EntityA;
            return true;
        }
        return false;
    }

    private bool AreDifferentPlayers(Entity unitEntity, Entity baseEntity)
    {
        var unitTag = unitLookup[unitEntity];
        var baseTag = baseLookup[baseEntity];

        return unitTag.Tag != baseTag.Tag;
    }
}
}