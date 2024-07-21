using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public class LaneManagerAuthoring : MonoBehaviour
    {
        public List<UnitPrefabSpawnWeight> UnitPrefabSpawnWeight;

        [Tooltip("Player 1 Lane 1,2,3; Player2 Lane 1,2,3")]
        public Transform[] SpawnLocations;

        public int MaximumUnitOnHand = 5;
        public float UnitSpawnStartDelay = 1f;
        public float UnitSpawnInterval = 5f;

        private UnitSpawnLocations[] spawnLocations;
        private List<UnitTypes> unitTypes;

        public class Baker : Baker<LaneManagerAuthoring>
        {
            public override void Bake(LaneManagerAuthoring authoring)
            {
                authoring.spawnLocations = new UnitSpawnLocations[authoring.SpawnLocations.Length];
                for (var i = 0; i < authoring.SpawnLocations.Length; i++)
                    authoring.spawnLocations[i].SpawnLocation = authoring.SpawnLocations[i].position;
                authoring.unitTypes = new List<UnitTypes>();
                foreach (var unitSpawn in authoring.UnitPrefabSpawnWeight)
                   
                        authoring.unitTypes.Add(new UnitTypes
                        {
                            Unit = GetEntity(unitSpawn.UnitPrefab,
                                TransformUsageFlags.Dynamic),
                            SpawnWeight = unitSpawn.SpawnWeight
                        });

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
                unitTypes.CopyFrom(authoring.unitTypes.ToArray());
            }
        }

        private void OnValidate()
        {
            var unitPrefabLength = Enum.GetNames(typeof(UnitType)).Length;
            while (UnitPrefabSpawnWeight.Count < unitPrefabLength) UnitPrefabSpawnWeight.Add(default);

            while (UnitPrefabSpawnWeight.Count > unitPrefabLength)
                UnitPrefabSpawnWeight.RemoveAt(UnitPrefabSpawnWeight.Count - 1);
        }
    }

    [Serializable]
    public class UnitPrefabSpawnWeight
    {
        public GameObject UnitPrefab;
        public int SpawnWeight;
    }

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
        public int SpawnWeight;
    }
}