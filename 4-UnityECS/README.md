# Unity ECS solution

In this project I've modified the original "UnityBalls" sample project to use Unity's Entity Component System.

## Creating and initialising entities in Unity

Creating entities and setting the component data is quite simple. Here's an example:

```csharp
EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

// Create ball archetype. This is basically a list of all components we want it to have.
EntityArchetype ballArchetype = entityManager.CreateArchetype(
    typeof(Translation),
    typeof(BallData));

// Instantiate entities.
NativeArray<Entity> entities = new NativeArray<Entity>(ballCount, Allocator.TempJob);
entityManager.CreateEntity(ballArchetype, entities);

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
```

Notice how similar this is to the ECS API we have been discussing up until now?

We first call `CreateArchetype` to create an archetype for our ball entities, and specify that we want them to have `BallData` and `Translation` components.

We then call `CreateEntity` to create the entities, and pass it the number of entities we want and a reference to our entity array.

We then iterate through all the entities, and call `SetComponentData` to set the actual component data. This is not the most efficient way to set the data though.
There are several ways to update a set of entities, such as `Entities.ForEach`. I will come back to this soon.

## What about the mesh data?

To use the EntityComponentSystem with the Unified Render Pipeline, we need to use the [Hybrid Renderer](https://docs.unity3d.com/Packages/com.unity.rendering.hybrid@latest/). This project is set up to use that already.

The hybrid renderer requires the entities to have a number of rendering-related component for it to render them. Luckily we don't have to create them manually.
All we need to do is to create a `RenderMeshDescription` and then pass it to the `RenderMeshUtility.AddComponents` function.

```csharp
var renderDesc = new RenderMeshDescription(
    ballMesh,
    ballMaterial,
    shadowCastingMode: ShadowCastingMode.Off,
    receiveShadows: false);
RenderMeshUtility.AddComponents(entityPrefab, entityManager, renderDesc);
```

However, if all the entities will have the same render data we don't want to do this for every single entity.
Instead, we can create one entity, set up the RenderMesh data and then use it as a prefab for creating other entities.
It can be done like this:

```csharp
// Create temporary entity prefab, form which we will instantiate all other entities
Entity entityPrefab = entityManager.CreateEntity(ballArchetype);

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
```

## The sample project

The scene contains a "BallSpawner" script which is responsible for creating all the entities.

We first define the "archetype" of our balls.
The archetype is basically just a collection of component types used by a set of entities, and is a way of grouping entities that share the same nature.
An entity can change archetype at runtime. So if you add a new component to it, it will change archetype.
Since component data is stored in arrays per archetype, changing archetype will result in the component data being moved to a new container.

We then create an entity prefab, which is a temporary entity that we use as a blueprint for the entities we want to spawn.
This is not necessary, but it saves some work (data that is the same for all entities can then simply be copied over).

After creating the entities (which are basically just IDs) we then set the component data of each entity.

## Updating the balls

The `BallMovementSystem` is responsible for updating the positions of the balls.
And before you start searching for references to this script: you won't find any.
Any script that inherits from `SystemBase` will automatically be initialised and start running once the game starts. In other words: it's magic.

We use `Entities.WithBurst().ForEach` to update the ball entities.
This function takes a lambda that will be executed once per entity.
The `WithBurst()`-call will make the [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@latest/) run and convert our code to optimised code using LLVM.
For more info about the bust compiler see [this talk](https://www.youtube.com/watch?v=Tzn-nX9hK1o), as well as [this talk](https://www.youtube.com/watch?v=BpwvXkoFcp8) for more details about SIMD.

Also, notice that the 3 if-chekcs that we used to bounce the balls off the walls now are gone.
Instead we use Unity.Mathematics' implementation of [step](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-step) to do this in a more vectorised manner. The `step` function will compare two paramets (`a` and `b`) and return 1 or 0 based on which value is greater. We modify the output a little bit, so that the direction will be multiplied with -1 if the position exceeds the box bounds, and 1 in all other cases.
