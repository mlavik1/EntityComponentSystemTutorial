# Basic idea of ECS: Data Oriented Balls

Here I show the basic idea of ECS, which is separating the data from the logic, and storing and accessing the data linearly - to improve cache efficiency.
Instead of lists of class references we now use lists of continuously allocated data.

## How to run the project

If using VSCode/VSCodium: Open the terminal and type: `code -r DataOrientedBalls`

## How it works

The code should be quite self explanatory. We store the ball data and positions in two separate arrays.
In ECS these would be the "components". The "entity" would then simply be the indices of these arrays.

To update the balls, we simply iterate over the two arrays, read the position, direction and speed, and calculate and store a new position value.

## Performance

Since we iterate over the lists and access the data linearly, cache efficiency will be good.

NOTE: We have to use `struct`s. `class` is a reference type, and if we used arrays of `class` instances then we would read pointers to memory stored elsewhere, and cache efficiency would not be good.

## Cache efficiency?

Reading RAM is relatively slow. When the CPU reads data from RAM, it will therefore read chunks of data and store them in a [CPU cache](https://en.wikipedia.org/wiki/CPU_cache).
These are smaller memory storages that are faster to access.
When an application requests some data, the data will be fetched from cache.
If the data is not present in the cache, we get a "cache miss", in which case the thread will stall until the data has been fetched from RAM.

The real benefit of these caches become ovious when accessing linearly allocated memory: For example when iterating over an array.
On the first iteration, a chunk of data might be read from RAM and stored in the cache. This is a slow operation. During the next few iterations we already have the data we need in cache, and we won't need to read from RAM.

Accessing data in a linear manner is therefore usually much faster than accessing sparsely allocated data.
An example of this in C# is the difference between storing data in an array of struct vs array of class.
In C# `struct` is a value type, and in an array of `struct` the memory will therefore be linearly allocated.
On the other hand `class` is a reference type, which is basically a pointer to memory stored elsewhere.
An array of class is therefore an array of pointers, and when iterating over it we won't be reading the data linearly.
See [this article](https://www.jacksondunstan.com/articles/3399) for a nice comparison!

If you want to learn more about how memory and CPU caches work, I can recommend [this](https://arnaudbrousseau.com/notes/what-every-programmer-should-know-about-memory-really.html) article.

## What's next?
So we have managed to implement a more efficient and cache friendly way of updating a list of moving objects. Isn't that good enough?
In our simple case: yes, it might be. But in a more complex project (such as in a game engine) we might want to have many entities with different components.
For example, all our entities might have a position, but only some of them are balls. Others might have other properties and data.
We could create separate entity lists and systems for each type of entity, but then we wouldn't be able to make systems that access/modify all entities that have a specific component (such as position).
So we need a slightly more advanced system for this: An Entity Component System! 