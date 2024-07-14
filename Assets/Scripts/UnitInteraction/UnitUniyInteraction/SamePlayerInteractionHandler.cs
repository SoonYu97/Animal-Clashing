using System.Threading;
using DefaultNamespace;
using UnitInteraction.Interfaces;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnitInteraction
{
    public class SamePlayerInteractionHandler : IUnitInteractionHandler
    {
        private ComponentLookup<Unit> unitLookup;
        private readonly ComponentLookup<LocalTransform> localTransformLookup;

        public SamePlayerInteractionHandler(ComponentLookup<Unit> unitLookup, ComponentLookup<LocalTransform> localTransformLookup)
        {
            this.unitLookup = unitLookup;
            this.localTransformLookup = localTransformLookup;
        }

        public void HandleInteraction(Entity entityA, Entity entityB)
        {
            StopFriendlyIfInRange(entityA, entityB, 1);
        }

        private void StopFriendlyIfInRange(Entity unit1, Entity unit2, float range)
        {
            if (!IsInRange(unit1, unit2, range)) return;

            var playerTag = unitLookup[unit1].Tag;
            if (playerTag == PlayerTag.Player1)
            {
                StopEntity(localTransformLookup[unit1].Position.x > localTransformLookup[unit2].Position.x ? unit2 : unit1);
            }
            else
            {
                StopEntity(localTransformLookup[unit1].Position.x < localTransformLookup[unit2].Position.x ? unit2 : unit1);
            }
        }

        private bool IsInRange(Entity unit1, Entity unit2, float range)
        {
            var distanceSq = math.distancesq(localTransformLookup[unit1].Position, localTransformLookup[unit2].Position);
            return range > distanceSq;
        }

        private void StopEntity(Entity unitEntity)
        {
            var unit = unitLookup.GetRefRW(unitEntity);
            Interlocked.Exchange(ref unit.ValueRW.CanMove, 0);
        }
    }

}