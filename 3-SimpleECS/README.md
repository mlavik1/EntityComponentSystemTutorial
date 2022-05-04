#  Simple ECS implementation

Here I show a simple and very naive implementation of ECS.

## How to run the project

If using VSCode/VSCodium: Open the terminal and type: `code -r EntityComponentSystem`

## How it works

The `EntityComponentSystem` has one array per component type. These arrays are pre-initialised with a size equal to the system's maximum entity count.

## Issues

- If the maximum entity count is large, the component arrays will take up *a lot* of memory!
- Large component lists => [Memory fragmentation](https://en.wikipedia.org/wiki/Fragmentation_(computing)).

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
