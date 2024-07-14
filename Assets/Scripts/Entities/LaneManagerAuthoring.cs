using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class LaneManagerAuthoring : MonoBehaviour
    {
        public GameObject[] UnitPrefabs;
        [Tooltip("Player 1 Lane 1,2,3; Player2 Lane 1,2,3")]
        public Transform[] SpawnLocations;
        public int MaximumUnitOnHand = 5;  
        public float UnitSpawnStartDelay = 1f;  
        public float UnitSpawnInterval = 5f;
        
        private UnitSpawnLocations[] spawnLocations;
        private UnitTypes[] unitTypes;
        
        public class Baker : Baker<LaneManagerAuthoring>
        {
            public override void Bake(LaneManagerAuthoring authoring)
            {
                authoring.spawnLocations = new UnitSpawnLocations[authoring.SpawnLocations.Length];
                for (var i = 0; i < authoring.SpawnLocations.Length; i++)
                {
                    authoring.spawnLocations[i].SpawnLocation = authoring.SpawnLocations[i].position;
                }
                authoring.unitTypes = new UnitTypes[authoring.UnitPrefabs.Length];
                for (var i = 0; i < authoring.UnitPrefabs.Length; i++)
                {
                    authoring.unitTypes[i].Unit = GetEntity(authoring.UnitPrefabs[i], TransformUsageFlags.Dynamic);
                }
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new LaneConfig
                {
                    UnitSpawnStartDelay = authoring.UnitSpawnStartDelay,
                    UnitSpawnInterval = authoring.UnitSpawnInterval,
                    MaximumUnitOnHand = authoring.MaximumUnitOnHand
                });
                var unitSpawnLocations = AddBuffer<UnitSpawnLocations>(entity);
                unitSpawnLocations.CopyFrom(authoring.spawnLocations);
                var unitTypes = AddBuffer<UnitTypes>(entity);
                unitTypes.CopyFrom(authoring.unitTypes);
            }
        }
    }
}