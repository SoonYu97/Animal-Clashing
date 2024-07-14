using System.Threading;
using DefaultNamespace;
using UnitInteraction.Interfaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnitInteraction
{
    public class DifferentPlayerInteractionHandler : IUnitInteractionHandler
    {
        private ComponentLookup<Unit> unitLookup;
        private readonly ComponentLookup<LocalTransform> localTransformLookup;
        private EntityCommandBuffer commandBuffer;
        private NativeQueue<UnitKilledEvent>.ParallelWriter eventQueueParallel;
        private readonly double currentTime;

        public DifferentPlayerInteractionHandler(
            ComponentLookup<Unit> unitLookup,
            ComponentLookup<LocalTransform> localTransformLookup,
            EntityCommandBuffer commandBuffer, 
            NativeQueue<UnitKilledEvent>.ParallelWriter eventQueueParallel, 
            double currentTime)
        {
            this.unitLookup = unitLookup;
            this.localTransformLookup = localTransformLookup;
            this.commandBuffer = commandBuffer;
            this.eventQueueParallel = eventQueueParallel;
            this.currentTime = currentTime;
        }

        public void HandleInteraction(Entity attacker, Entity defender)
        {
            var attackRangeSq = GetAttackRangeSq(attacker);
            StopAndAttackIfInRange(attacker, defender, attackRangeSq);
            StopAndAttackIfInRange(defender, attacker, GetAttackRangeSq(defender));
        }

        private float GetAttackRangeSq(Entity attacker)
        {
            return unitLookup[attacker].AttackRange * unitLookup[attacker].AttackRange;
        }

        private void StopAndAttackIfInRange(Entity attacker, Entity defender, float range)
        {
            if (!IsInRange(attacker, defender, range)) return;
            StopEntity(attacker);
            AttackEntity(attacker, defender);
        }

        private bool IsInRange(Entity unit1, Entity unit2, float range)
        {
            float distanceSq = math.distancesq(localTransformLookup[unit1].Position, localTransformLookup[unit2].Position);
            return range > distanceSq;
        }

        private void StopEntity(Entity unitEntity)
        {
            var unit = unitLookup.GetRefRW(unitEntity);
            Interlocked.Exchange(ref unit.ValueRW.CanMove, 0);
        }

        private void AttackEntity(Entity attacker, Entity defender)
        {
            var attackerUnit = unitLookup.GetRefRW(attacker);
            var defenderUnit = unitLookup.GetRefRW(defender);

            var timeBetweenAttacks = 60.0 / attackerUnit.ValueRO.AttackRate; // Convert rate to time between attacks in seconds

            if (!(currentTime - attackerUnit.ValueRO.LastAttackTime >= timeBetweenAttacks)) return;

            Interlocked.Exchange(ref defenderUnit.ValueRW.Health, defenderUnit.ValueRO.Health - attackerUnit.ValueRO.Strength);
            Interlocked.Exchange(ref attackerUnit.ValueRW.LastAttackTime, currentTime);

            DestroyEntityIfHealthLessThanZero(defender);
        }

        private void DestroyEntityIfHealthLessThanZero(Entity defender)
        {
            if (!(unitLookup.GetRefRO(defender).ValueRO.Health <= 0)) return;
            commandBuffer.DestroyEntity(defender);
            eventQueueParallel.Enqueue(new UnitKilledEvent
            {
                Score = 1,
                Tag = unitLookup[defender].Tag
            });
        }
    }
}
