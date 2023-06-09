using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace DOTS
{
    [BurstCompile]
    [UpdateBefore(typeof(AttackSystem))]
    [UpdateAfter(typeof(BeeWallCollisionSystem))]
    public partial struct TargetSystem : ISystem
    {
        private EntityQuery team1Alive;
        private EntityQuery team2Alive;

        public void OnCreate(ref SystemState state)
        {
            team1Alive = SystemAPI.QueryBuilder().WithAll<TeamOne>().WithNone<Dead>().Build();
            team2Alive = SystemAPI.QueryBuilder().WithAll<TeamTwo>().WithNone<Dead>().Build();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var team1Entities =
                team1Alive.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var dep1);
            var team2Entities =
                team2Alive.ToEntityListAsync(state.WorldUpdateAllocator, state.Dependency, out var dep2);

            state.Dependency = new TargetJob
            {
                team1Enemies = team2Entities,
                team2Enemies = team1Entities
            }.ScheduleParallel(JobHandle.CombineDependencies(dep1, dep2));
        }


        [BurstCompile]
        [WithNone(typeof(Dead))]
        public partial struct TargetJob : IJobEntity
        {
            [ReadOnly] public NativeList<Entity> team1Enemies;
            [ReadOnly] public NativeList<Entity> team2Enemies;

            private void Execute(ref RandomComponent random, ref Target target, in Team team)
            {
                if (target.enemyTarget == Entity.Null)
                {
                    var enemies = team.Value == 1 ? team1Enemies : team2Enemies;
                    int newTarget = random.generator.NextInt(0, enemies.Length);
                    target.enemyTarget = enemies[newTarget];
                }
            }
        }
    }
}