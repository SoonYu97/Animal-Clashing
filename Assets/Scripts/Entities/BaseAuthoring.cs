using UnityEngine;
using Unity.Entities;

namespace DefaultNamespace
{
    public class BaseAuthoring : MonoBehaviour
    {
        public PlayerTag Tag;
        
        public class Baker : Baker<BaseAuthoring>
        {
            public override void Bake(BaseAuthoring authoring)
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