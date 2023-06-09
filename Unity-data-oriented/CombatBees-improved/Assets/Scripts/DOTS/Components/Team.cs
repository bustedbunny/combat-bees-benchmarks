using Unity.Entities;

public struct TeamOne : IComponentData { }

public struct TeamTwo : IComponentData { }

public struct Team : IComponentData
{
    public byte Value;
}