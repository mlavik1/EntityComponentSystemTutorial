# Entity Component System Tutorial

In this tutorial I will try to explain the basics of what an entity component system is, why we need it, how it can be implemented (and why it's not as easy as it might seem, and how to use Unity's entity system).

The tutorial has 4 parts, with their own README files and sample projects.

## Part 1: The problem

Sample project simulating balls moving inside a box, without using ECS.

[See the tutorial here](1-TheProblem/README.md)

## Part 2: Basic idea

Here I explain the basic idea behind ECS, and what problems it tries to solve.
I also show how to can write cache-friendly code in a similar manner to how ECS works.

[See the tutorial here](2-BasicIdea/README.md)

## Part 3: A naive ECS implementation

How about making your own ECS implementation? It's more difficult than you might think!
Here I show a very basic ECS implementation, and explain why this is not a good enough implementation

[See the tutorial here](3-SimpleECS/README.md)

## Part 4: Unity ECS solution

Sample project simulating balls moving inside a box, now using Unity ECS.

[See the tutorial here](4-UnityECS/README.md)
