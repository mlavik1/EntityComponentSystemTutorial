#  Simple ECS implementation

Here I show a simple and very naive implementation of ECS.

Jumping straight into Unity's entity system can be confusing, since it will introduce you to a number of new concepts, such as "archetypes", "entities", and "components".

First, lets take a look at how an ECS could be implemented, and what the different parts of such a system would be.

Remember: Our goal is to store data in a way that allows to access the data in a linear manner.

## How to run the project

If using VSCode/VSCodium: Open the terminal and type: `code -r EntityComponentSystem`

## How it works

The `EntityComponentSystem` is responsible for creating and updating entities and components.
On initialisation we create a queue of available entities, which are basically just IDs/integers.
When creating a new entity, we will simply dequeue an entity ID from this queue.
These entities are used as index to the component arrays.

We create one array per component type.
These arrays are pre-initialised with a fixed size equal to the system's maximum entity count.
The component index will be equal to the entity's value. In other words: We will have as many components as we have entities, and the size is fixed.

## Benefits

The component data is now stored linearly.
In theory: If we need to iterate over all entities, we would be able to access the component data in a linear manner.
However, in practice it's not so nice, since the `GetComponent` function needs to look up the component list in a dictionary.

## Issues

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
I've made another example project that follows this approach, but in C++. See it [here](https://codeberg.org/matiaslavik/SimpleEntityComponentSystem/src/branch/main/ecs/details/entity_component_container.h).

A better solution would be to identify entities by their "archetype", and store component data per archetype.
An archetype is basically a set of component types that the entity has.
Entities of the same archetype (for example: entities that have both a `Position` and `BallData` component) have their components stored in a common list.
When iterating through entities of the same archetype (which is usually what we want to do) we can then access the component data linearly.

Updating and accessing the components of a given entity ID would then be slightly more expensive, so we also want a better way of accessing the data.
In Unity you can use `Entities.ForEach` to do this, and with the Burst compiler you can get pretty good performance from this.

Another problem that we want to avoid is having too large component lists.
This can cause [memory fragmentation](https://en.wikipedia.org/wiki/Fragmentation_(computing)) issues, so we want to segment the component lists into smaller chunks.
This also happens to be what Unity does in it's ECS implementation.
