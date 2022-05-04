using System.Numerics;

class Program
{
    public struct Position
    {
        public Vector3 position;
    }

    public struct BallData
    {
        public Vector3 direction;
        public float speed;
    }

    static void Main(string[] args)
    {
        Random rand = new Random();
        
        // Create balls
        const int ballCount = 32;
        BallData[] ballsData = new BallData[ballCount];
        Position[] positions = new Position[ballCount];

        // Set ball data
        for (int ballIndex = 0; ballIndex < ballCount; ++ballIndex)
        {
            positions[ballIndex] = new Position(){ position = new Vector3(rand.NextSingle(), rand.NextSingle(), rand.NextSingle()) * 2.0f - Vector3.One };
            ballsData[ballIndex] = new BallData(){ speed = rand.NextSingle() * 40.0f + 10.0f };
        }

        const float deltaTime = 0.1f; // example

        // Iterate over all balls and update their positions
        for (int ballIndex = 0; ballIndex < ballCount; ++ballIndex)
        {
            positions[ballIndex].position += ballsData[ballIndex].direction * ballsData[ballIndex].speed * deltaTime;
        }
    }
}
