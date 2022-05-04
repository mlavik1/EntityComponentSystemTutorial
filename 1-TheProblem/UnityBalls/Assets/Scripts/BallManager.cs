using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public GameObject ballPrefab;
    public int ballCount = 64;

    private List<Ball> balls = new List<Ball>();

    void Start()
    {
        // Spawn all the balls
        for (int i = 0; i < ballCount; ++i)
        {
            GameObject ballObject = Instantiate(ballPrefab);
            Ball ball = ballObject.AddComponent<Ball>();
            // Set random direction
            ball.direction = Vector3.Normalize(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)));
            // Set random speed
            ball.speed = Random.Range(10.0f, 50.0f);
            balls.Add(ball);
        }
    }

    void Update()
    {
        // Box extents (balls should always be inside this box)
        const float boxMin = -SimulationSettings.BoxExtents / 2;
        const float boxMax = SimulationSettings.BoxExtents / 2;

        for (int i = 0; i < ballCount; ++i)
        {
            Ball ball = balls[i];

            // Calculate new position
            Vector3 newPos = ball.transform.position + ball.direction * ball.speed * Time.deltaTime;

            // Handle ball collision with box walls
            if (Mathf.Abs(newPos.x) >= boxMax)
                ball.direction.x *= -1.0f;
            if (Mathf.Abs(newPos.y) >= boxMax)
                ball.direction.y *= -1.0f;
            if (Mathf.Abs(newPos.z) >= boxMax)
                ball.direction.z *= -1.0f;

            // Clamp new position, so we don't leave the box
            newPos.x = Mathf.Clamp(newPos.x, boxMin, boxMax);
            newPos.y = Mathf.Clamp(newPos.y, boxMin, boxMax);
            newPos.z = Mathf.Clamp(newPos.z, boxMin, boxMax);

            ball.transform.position = newPos;
        }
    }
}
