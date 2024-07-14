using Unity.Entities;

namespace UnitInteraction.Interfaces
{
    public interface IUnitInteractionHandler
    {
        void HandleInteraction(Entity entityA, Entity entityB);
    }
}