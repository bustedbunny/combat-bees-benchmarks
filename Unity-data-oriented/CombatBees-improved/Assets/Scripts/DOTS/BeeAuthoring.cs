using Unity.Entities;
using UnityEngine;

namespace DOTS
{
    public class BeeAuthoring : MonoBehaviour
    {
        public byte team;

        public class BeeAuthoringBaker : Baker<BeeAuthoring>
        {
            public override void Bake(BeeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<Velocity>(entity);
                AddComponent<Target>(entity);
                AddComponent<RandomComponent>(entity);
                AddComponent(entity, new Team() { Value = authoring.team });
                if (authoring.team == 1)
                {
                    AddComponent<TeamOne>(entity);
                }
                else
                {
                    AddComponent<TeamTwo>(entity);
                }
            }
        }
    }
}