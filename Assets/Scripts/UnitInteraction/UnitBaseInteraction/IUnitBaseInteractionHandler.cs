using Unity.Physics;

namespace UnitInteraction
{
    public interface IUnitBaseInteractionHandler
    {
        void HandleInteraction(TriggerEvent triggerEvent);
    }
}