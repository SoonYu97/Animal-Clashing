using Unity.Entities;

namespace DefaultNamespace
{
    public struct GameState : IComponentData
    {
        public State Value;

        public enum State
        {
            Start,
            Playing,
            GameOver
        }
    }
}