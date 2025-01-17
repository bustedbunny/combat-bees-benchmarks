using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace DOTS
{
    [BurstCompile]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(AttackSystem))]
    public partial struct BeeMovementSystem : ISystem
    {
        private EntityQuery team1Bees;
        private EntityQuery team2Bees;

        public void OnCreate(ref SystemState state)
        {
            team1Bees = SystemAPI.QueryBuilder()
                .WithAll<TeamOne, Team, LocalToWorld, Velocity, RandomComponent>().WithNone<Dead>().Build();
            team2Bees = SystemAPI.QueryBuilder()
                .WithAll<TeamTwo, Team, LocalToWorld, Velocity, RandomComponent>().WithNone<Dead>().Build();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var team1Transforms =
                team1Bees.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob, state.Dependency, out var dep1);
            var team2Transforms =
                team2Bees.ToComponentDataListAsync<LocalToWorld>(Allocator.TempJob, state.Dependency, out var dep2);

            state.Dependency = new MovementJob
            {
                deltaTime = state.WorldUnmanaged.Time.DeltaTime,
                Team1Transforms = team1Transforms,
                Team2Transforms = team2Transforms,
            }.ScheduleParallel(JobHandle.CombineDependencies(dep1, dep2));

            team1Transforms.Dispose(state.Dependency);
            team2Transforms.Dispose(state.Dependency);
        }

        [BurstCompile]
        [WithNone(typeof(Dead))]
        public partial struct MovementJob : IJobEntity
        {
            public float deltaTime;
            [ReadOnly] public NativeList<LocalToWorld> Team1Transforms;
            [ReadOnly] public NativeList<LocalToWorld> Team2Transforms;

            // IJobEntity generates a component data query based on the parameters of its `Execute` method.
            // This example queries for all Spawner components and uses `ref` to specify that the operation
            // requires read and write access. Unity processes `Execute` for each entity that matches the
            // component data query.
            private void Execute(ref LocalTransform transform, ref Velocity velocity, ref RandomComponent random,
                in Team team)
            {
                float3 randomVector;
                randomVector.x = random.generator.NextFloat() * 2.0f - 1.0f;
                randomVector.y = random.generator.NextFloat() * 2.0f - 1.0f;
                randomVector.z = random.generator.NextFloat() * 2.0f - 1.0f;

                velocity.Value += randomVector * (Data.flightJitter * deltaTime);
                velocity.Value *= (1f - Data.damping * deltaTime);

                var aliveBeesCount = math.select(Team2Transforms.Length, Team1Transforms.Length, team.Value == 1);
                var allyPositions = team.Value == 1 ? Team1Transforms : Team2Transforms;

                //Move towards random ally
                float3 beePosition = transform.Position;
                int allyIndex = random.generator.NextInt(aliveBeesCount);
                var allyPosition = allyPositions[allyIndex].Position;
                float3 delta = allyPosition - beePosition;
                float dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                dist = math.max(0.01f, dist);
                velocity.Value += delta * (Data.teamAttraction * deltaTime / dist);

                //Move away from random ally
                allyIndex = random.generator.NextInt(aliveBeesCount);
                allyPosition = allyPositions[allyIndex].Position;
                delta = allyPosition - beePosition;
                dist = math.sqrt(delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);
                dist = math.max(0.01f, dist);
                velocity.Value -= delta * (Data.teamRepulsion * deltaTime / dist);

                var rotation = transform.Rotation;
                var targetRotation = quaternion.LookRotation(math.normalize(velocity.Value), math.up());
                rotation = math.nlerp(rotation, targetRotation, deltaTime * 4);
                transform.Rotation = rotation;
            }
        }
    }
}