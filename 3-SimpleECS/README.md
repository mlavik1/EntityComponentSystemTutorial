#  Simple ECS implementation

Here I show a simple and very naive implementation of ECS.

Jumping straight into Unity's entity system can be confusing, since it will introduce you to a number of new concepts, such as "archetypes", "entities", and "components".

First, lets take a look at how an ECS could be implemented, and what the different parts of such a system would be.

## Goals

Our goal is to store data in a way that allows to access the data in a linear manner.

We want to be able to do the following:
- Create an entity
- Create a number of entities
- Set component data on each entity
- Iterate over entities and their components, in a linear manner

The interface could look something like this:
```csharp
List<Entity> entities = ecs.CreateEntities(entityCount);

foreach (int entity in entities)
{
    BallData ballData = ecs.GetComponent<BallData>(entity);
    ecs.SetComponent(entity, new PositionData(...));
}
```

## How to run the project

The tutorial contains two projects:
- EntityComponentSystem (first attempt on ECS implementation)
- EntityComponentSystemImproved (same, but with optimised component access)

If using VSCode/VSCodium: Open the terminal and type: `code -r EntityComponentSystem`

## How it works

The `EntityComponentSystem` is responsible for creating and updating entities and components.
On initialisation we create a queue of available entities, which are basically just IDs/integers.
When creating a new entity, we will simply dequeue an entity ID from this queue.
These entities are used as index to the component arrays.

The components are stored in an array in the `ComponentArray` class. We create one array per component type.
These arrays are pre-initialised with a fixed size equal to the system's maximum entity count.
This ensures that we can index the components by the entity ID, to ensure linear memory access. The component index will be equal to the entity's value.

The `ComponentArray` instances are stored in a dictionary, where the `Type` of the component is used as key.

The `GetComponentArray` function gets the component array of a specified component type.
It uses the `Type` of the component as a key, and looks up the component array in the dictionary of component arrays.
To get the component of a specific entity, we simply have to do: `return compArr.components[entity]`.

So is this an efficient implementation? Not really.
The components of a `ComponentArray` can indeed be access linearly, but to get the component array of an entity we need to look it up in a dictionary.
It would be nicer if we could instead generate an index for the component `Type`, and store the component arrays in an array instead. Actually, we can! Using some template magic.

We first define the following classes:
```csharp
private class ComponentIndexCounter
{
    public static int guid = 0;
}

private class ComponentIndex<T>
{
    public static int guid = ComponentIndexCounter.guid++;
}
````

To get the index of a specific component type we can now simply call: `int compTypeIndex = ComponentIndex<T>.guid;`
The static `guid` variable will only be initialised once for each type `T`, so we will always get the same index.

See the `EntityComponentSystemImproved` project for the full implementation.

## Benefits

The component data is now stored linearly.
In theory: If we need to iterate over all entities, we would be able to access the component data in a linear manner.
However, we are forced to keep track of all the entity ID's ourselces, and if we start removing and adding new entities we will no longer have perfectly linearly allocated component data.

**Example:**
- We first create entities 0 to 5
- We then remove entity 2 and 3
- We create a new entity, which will return entity 2 (since it was freed above)
- Our entity list is now: 0, 1, 4, 5, 2
- We now have a "hole" in the component array, and the entity list is no longer sorted. We would have to sort it for better performance.

## Other issues

- If the maximum entity count is large, the component arrays will take up *a lot* of memory!
Say we want to allow up to a million entities, but only 10 of them have a `BallData` component.
With this implementation we will waste a lot of memory by storing a list of 1 million BallData components, that are never used.
- Large component lists => [Memory fragmentation](https://en.wikipedia.org/wiki/Fragmentation_(computing)).
Very large arrays is usually something you want to avoid.
If your system has 8 GB of RAM and you have 4 GB available, chances are that you won't be able to allocate a 1GB array, because memory is fragmented and you don't have 1GB of continous available memory.
You're more likely to have smaller chunks of available RAM many places.

## Possible imrpovements

Instead of having component lists of fixed size, we could instead start with an empty lists and grow them on demand.
We would then also need to store two other maps: One for getting the Component index of an entity, and one for getting the Entity of a component index. This would, however, add a performance cost when accessing/updating components.
I've made another example project that follows a similar approach, but in C++. See it [here](https://codeberg.org/matiaslavik/SimpleEntityComponentSystem/src/branch/main/ecs/details/entity_component_container.h).

A better solution would be to identify entities by their "archetype", and store component data per archetype.
An archetype is basically a set of component types that the entity has.
Entities of the same archetype (for example: entities that have both a `Position` and `BallData` component) have their components stored in a common list.
When iterating through entities of the same archetype (which is usually what we want to do) we can then access the component data linearly.

Updating and accessing the components of a given entity ID would then be slightly more expensive, so we also want a better way of accessing the data.
In Unity you can use `Entities.ForEach` to do this, and with the Burst compiler you can get pretty good performance from this.
After all, we don't want the user to keep track of all the entity IDs themselves. The library should provide functions for accesing and iterating over them.

Another problem that we want to avoid is having too large component lists.
This can cause [memory fragmentation](https://en.wikipedia.org/wiki/Fragmentation_(computing)) issues, so we want to segment the component lists into smaller chunks.
This also happens to be what Unity does in it's ECS implementation.

This would introduce a new and improved ECS interface, which could be used like this:
```csharp
Archetype archetype = new Archetype(typeof(PositionData), typeof(BallData)):
List<Entity> entities = ecs.CreateEntities(archetype, entityCount);

foreach (Entity entity in entities)
{
    ecs.SetComponent(entity, new PositionData(...));
    ecs.SetComponent(entity, new BallData(...));
}

ecs.ForEach((ref PositionData position, ref BallData ballData) =>
{
    position.position += ballData.direction * ballData.speed * deltaTime;
}
```
