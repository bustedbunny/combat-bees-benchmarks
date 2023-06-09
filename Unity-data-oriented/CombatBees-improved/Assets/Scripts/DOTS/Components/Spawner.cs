using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Serialization;

public struct Spawner : IComponentData
{
    public Entity TeamOneBee;
    public Entity TeamTwoBee;
    public float3 Team1SpawnPosition;
    public float3 Team2SpawnPosition;
}