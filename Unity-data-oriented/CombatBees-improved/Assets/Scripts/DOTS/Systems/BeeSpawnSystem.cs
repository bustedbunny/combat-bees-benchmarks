using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Core;

namespace DOTS
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(DeadBeesSystem))]
    public partial struct BeeSpawnSystem : ISystem
    {
        private EntityQuery team1Alive;
        private EntityQuery team2Alive;
        private EntityQuery team1Dead;
        private EntityQuery team2Dead;

        public void OnCreate(ref SystemState state)
        {
            team1Alive = SystemAPI.QueryBuilder().WithAll<TeamOne, Team>().WithNone<Dead>().Build();
            team2Alive = SystemAPI.QueryBuilder().WithAll<TeamTwo, Team>().WithNone<Dead>().Build();
            team1Dead = SystemAPI.QueryBuilder().WithAll<TeamOne, Team, Dead>().Build();
            team2Dead = SystemAPI.QueryBuilder().WithAll<TeamTwo, Team, Dead>().Build();
        }

        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int team1AliveCount = team1Alive.CalculateEntityCount();
            int team1DeadCount = team1Dead.CalculateEntityCount();
            int team1BeeCount = team1AliveCount + team1DeadCount;


            int team2AliveCount = team2Alive.CalculateEntityCount();
            int team2DeadCount = team2Dead.CalculateEntityCount();
            int team2BeeCount = team2AliveCount + team2DeadCount;

            var em = state.EntityManager;

            var timeData = SystemAPI.Time;

            foreach (var spawnerRef in SystemAPI.Query<RefRW<Spawner>>())
            {
                ref var spawner = ref spawnerRef.ValueRW;

                int beesToSpawnTeam1 = Data.beeStartCount / 2 - team1BeeCount;

                for (int i = 0; i < beesToSpawnTeam1; i++)
                {
                    Entity newEntity = em.Instantiate(spawner.TeamOneBee);
                    var rand = new RandomComponent();
                    rand.generator.InitState((uint)((i + 1) * (timeData.ElapsedTime + 1.0) * 57131));
                    var transform = LocalTransform.FromPosition(spawner.Team1SpawnPosition);
                    transform.Scale = rand.generator.NextFloat(Data.minBeeSize, Data.maxBeeSize);
                    SystemAPI.SetComponent(newEntity, transform);
                    SystemAPI.SetComponent(newEntity, rand);
                }


                int beesToSpawnTeam2 = Data.beeStartCount / 2 - team2BeeCount;

                for (int i = 0; i < beesToSpawnTeam2; i++)
                {
                    Entity newEntity = em.Instantiate(spawner.TeamTwoBee);
                    var rand = new RandomComponent();
                    rand.generator.InitState((uint)((i + 1) * (timeData.ElapsedTime + 1.0) * 33223));
                    var transform = LocalTransform.FromPosition(spawner.Team2SpawnPosition);
                    transform.Scale = rand.generator.NextFloat(Data.minBeeSize, Data.maxBeeSize);
                    SystemAPI.SetComponent(newEntity, transform);
                    SystemAPI.SetComponent(newEntity, rand);
                }
            }
        }
    }
}