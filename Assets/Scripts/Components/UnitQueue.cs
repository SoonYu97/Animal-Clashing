using Unity.Entities;
using Unity.Collections;

namespace DefaultNamespace
{
    public struct UnitQueue : IComponentData
    {
        public FixedList32Bytes<int> Queue1;
        public FixedList32Bytes<int> Queue2;
    }
}
