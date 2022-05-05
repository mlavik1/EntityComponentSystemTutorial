using System.Numerics;

class Program
{
    public struct PositionData : IComponentData
    {
        public Vector3 position;
    }

    public struct BallData : IComponentData
    {
        public Vector3 direction;
        public float speed;
    }

    static void Main(string[] args)
    {
        Random rand = new Random();
        
        // Create entities
        const int entityCount = 32;
        EntityComponentSystem entitySystem = new EntityComponentSystem(128);
        List<int> entities = entitySystem.AddEntities(entityCount);

        // Set components
        foreach (int entity in entities)
        {
            entitySystem.SetComponent<PositionData>(entity, new PositionData(){ position = new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()) * 2.0f - Vector3.One });
            entitySystem.SetComponent<BallData>(entity, new BallData(){ speed = rand.NextSingle() * 40.0f + 10.0f, direction = Vector3.Normalize(new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()) * 2.0f - Vector3.One) });
        }

        const float deltaTime = 0.1f; // example

        // Iterate over all entities and update their positions
        foreach (int entity in entities)
        {
            PositionData position = entitySystem.GetComponent<PositionData>(entity);
            BallData ballData = entitySystem.GetComponent<BallData>(entity);
            position.position += ballData.direction * ballData.speed * deltaTime;
            entitySystem.SetComponent<PositionData>(entity, position);
        }
    }
}
