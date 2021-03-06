using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public struct GravityWellComponent_GO_ECS : IComponentData
{
    public float Strength;
    public float Radius;
    // Include position of gravity well so all data accessible in one location
    public float3 Position;
}

/// <summary>
/// Component system for moving all the balls.
/// The system will automatically run (no need to reference it anywhere).
/// Boxes move with a constant speed in a random direction inside a box (AABB). They will bounce off the walls.
/// </summary>
public partial class BallMovementSystem : SystemBase
{
    /// <summary>
    /// Get intersection with the axis alligned bounding box.
    /// </summary>
    /// <returns>tNear, tFar</returns>
    private static float2 intersectAABB(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
    {
        float3 tMin = (boxMin - rayOrigin) / rayDir;
        float3 tMax = (boxMax - rayOrigin) / rayDir;
        float3 t1 = math.min(tMin, tMax);
        float3 t2 = math.max(tMin, tMax);
        float tNear = math.max(math.max(t1.x, t1.y), t1.z);
        float tFar = math.min(math.min(t2.x, t2.y), t2.z);
        return new float2(tNear, tFar);
    }

    protected override void OnUpdate()
    {
        // Store these variables in local variables, since WithBurst().ForEach() can't capture "this".
        float boxExtents = SimulationSettings.BoxExtents;
        float deltaTime = Time.DeltaTime;

        // Iterate over all entities that have Translation and BallData, with a Burst-compiled lambda (all code below will be compiled with Burst).
        Entities.WithBurst().ForEach((ref Translation trans, ref BallData boidData) =>
        {
            // Box extents
            float boxMin = -boxExtents / 2;
            float boxMax = boxExtents / 2;

            // Distance to travel this frame
            float currStep = boidData.speed * deltaTime;

            // Find intersection with axis aligned bounding box
            float3 rayOrigin = trans.Value;
            float3 rayDir = boidData.direction;
            float2 aabbIntersection = intersectAABB(rayOrigin, rayDir, boxMin, boxMax);
            float tInters = aabbIntersection.y;

            // Clamp dinstance to make sure we don't leave the box
            float t = math.min(tInters, currStep);

            float3 newPos = trans.Value + boidData.direction * t;

            // Bounce off the wall.
            // This is a vectorized version of:
            //   if(abs(newPos.x) >= boxMax)
            //      direction.x *= -1.0f
            //   ... for x, y and z.
            boidData.direction *= (math.step(new float3(boxMax, boxMax, boxMax), math.abs(newPos)) * -2 + 1);

            // Set new position
            trans.Value = newPos;
        }).Run();
    }
}
