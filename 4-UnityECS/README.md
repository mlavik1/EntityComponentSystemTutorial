# Unity ECS solution

In this project I've modified the original "UnityBalls" sample project to use Unity's Entity Component System.

## Creating the entities

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
Any script that inherits from `IComponentData` will automatically be initialised and start running once the game starts. In other words: it's magic.

We use `Entities.WithBurst().ForEach` to update the ball entities.
This function takes a lambda that will be executed once per entity.
The `WithBurst()`-call will make the [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@latest/) run and convert our code to optimised code using LLVM.
For more info about the bust compiler see [this talk](https://www.youtube.com/watch?v=Tzn-nX9hK1o), as well as [this talk](https://www.youtube.com/watch?v=BpwvXkoFcp8) for more details about SIMD.

Also, notice that the 3 if-chekcs that we used to bounce the balls off the walls now are gone.
Instead we use Unity.Mathematics' implementation of [step](https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-step) to do this in a more vectorised manner. The `step` function will compare two paramets (`a` and `b`) and return 1 or 0 based on which value is greater. We modify the output a little bit, so that the direction will be multiplied with -1 if the position exceeds the box bounds, and 1 in all other cases.
