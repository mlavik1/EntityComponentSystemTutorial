class Program
{
    public struct PositionComp : IComponentData
    {
        public float x, y;
    }

    public struct PlayerDataComp : IComponentData
    {
        public string name;
        public int health;
    }

    static void Main(string[] args)
    {
        const int entityCount = 32;
        EntityComponentSystem entitySystem = new EntityComponentSystem(128);
        List<int> entities = entitySystem.AddEntities(entityCount);
        Random rand = new Random();
        foreach (int entity in entities)
        {
            entitySystem.SetComponent<PositionComp>(entity, new PositionComp(){ x = rand.NextSingle(), y = rand.NextSingle() });
            entitySystem.SetComponent<PlayerDataComp>(entity, new PlayerDataComp(){ health = 100, name = "player" + entity });
        }

        foreach (int entity in entities)
        {
            PositionComp position = entitySystem.GetComponent<PositionComp>(entity);
            PlayerDataComp playerData = entitySystem.GetComponent<PlayerDataComp>(entity);
            Console.WriteLine($"{playerData.name} : health: {playerData.health}, position: {position.x}, {position.x}");
        }
    }
}
