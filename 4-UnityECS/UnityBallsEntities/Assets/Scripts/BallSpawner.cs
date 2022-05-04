using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;

public class BallSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public int ballCount = 64;

    void Start()
    {
        // Get mesh and material from ball prefab
        Mesh ballMesh = ballPrefab.GetComponent<MeshFilter>().sharedMesh;;
        Material ballMaterial = ballPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Create ball archetype. This is basically a list of all components we want it to have.
        EntityArchetype boidArchetype = entityManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(BallData));

        // Create temporary entity prefab, form which we will instantiate all other entities
        Entity entityPrefab = entityManager.CreateEntity(boidArchetype);

        // The hybrid renderer requires a set of components to render a mesh. Add these components, and set the mesh and material.
        var renderDesc = new RenderMeshDescription(
            ballMesh,
            ballMaterial,
            shadowCastingMode: ShadowCastingMode.Off,
            receiveShadows: false);
        RenderMeshUtility.AddComponents(entityPrefab, entityManager, renderDesc);

        // Instantiate entities.
        NativeArray<Entity> entities = new NativeArray<Entity>(ballCount, Allocator.TempJob);
        entityManager.Instantiate(entityPrefab, entities);

        // Set the components of all spawned entities.
        foreach (Entity entity in entities)
        {
            // Set custom ball data (speed and direction)
            entityManager.SetComponentData(entity, new BallData
            {
                direction = math.normalize(new float3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f))),
                speed = Random.Range(10.0f, 50.0f)
            });

            // Set position
            Translation translation = new Translation();
            translation.Value = new float3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)) * SimulationSettings.BoxExtents;
            entityManager.SetComponentData(entity, translation);
        }

        entityManager.DestroyEntity(entityPrefab);

        entities.Dispose();
    }

    void Update()
    {
        
    }
}
