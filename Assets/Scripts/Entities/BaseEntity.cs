using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class BaseEntity : MonoBehaviour
    {
        public PlayerTag Tag;
        
        public class Baker : Baker<BaseEntity>
        {
            public override void Bake(BaseEntity authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Base
                {
                    Tag = authoring.Tag
                });
            }
        }
    }
}