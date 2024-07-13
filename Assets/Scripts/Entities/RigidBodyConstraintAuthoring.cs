using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

namespace DefaultNamespace
{
    // This system is intended as a workaround for the rigidbody constraint of the Unity Physics which is not working.
    public class RigidBodyConstraintAuthoring : MonoBehaviour
    {
        public bool3 FreezePosition;
        public bool3 FreezeRotation;

        public class Baker : Baker<RigidBodyConstraintAuthoring>
        {
            public override void Bake(RigidBodyConstraintAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new RigidBodyConstraint
                {
                    FreezePosition = authoring.FreezePosition,
                    FreezeRotation = authoring.FreezeRotation,
                    PositionCache = authoring.transform.position,
                    RotationCache = authoring.transform.rotation
                });
            }
        }
    }

    public struct RigidBodyConstraint : IComponentData
    {
        public bool3 FreezePosition;
        public bool3 FreezeRotation;
        public float3 PositionCache;
        public quaternion RotationCache;
    }
}