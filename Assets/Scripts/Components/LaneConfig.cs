using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DefaultNamespace
{
    public struct LaneConfig : IComponentData
    {
        public int MaximumUnitOnHand;
        public float UnitSpawnStartDelay;
        public float UnitSpawnInterval;
    }
    
    [Serializable]
    public struct UnitSpawnLocations : IBufferElementData
    {
        public float3 SpawnLocation;
    }
    
    [Serializable]
    public struct UnitTypes : IBufferElementData
    {
        public Entity Unit;
    }
}
