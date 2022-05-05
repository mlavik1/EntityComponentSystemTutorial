# UnityBalls sample

This example project the movement of a number of balls inside of a box.
The bals have a random position, speed and direction, and will bounce off the walls.

## How to run the project

The project was made with Unity 2020.3.33f1, but other versions should work as well.

Simply open the SampleScene and click play.

You can change the ball count in the BallManager game object.

## How it works

The `BallManager` script is responsible for spawning and updating the balls.

On start we spawn a number of ball instances, based on a ball prefab.
Each ball has a random direction and speed, stored in the `Ball` script.

On update we iterate over the list of balls, and update their positions like this: `position = position + direction * speed * deltaTime`.

To make the balls bounce off the walls of the box, we flip the x,y,z coordinates when they exceed the box bounds.

## Performance issues

- We iterate over a list of Ball components to update the balls.
These are basically pointers to data stored elsewhere in memory, which results in poor [cache](https://en.wikipedia.org/wiki/CPU_cache) efficiency.
When updating the positions of the balls we modify `Ball.transform` which also is a reference to data stored another place in memory.
- We block the main thread while updating the balls (would be nice to do this async!).
- We use 3 if-checks in the code for making the balls bounce off the walls.
This can in fact be vectorized optimised with [SIDM](https://en.wikipedia.org/wiki/Single_instruction,_multiple_data) operations.
See the 4th sample project for how to do this.

## Possible solution

We need to store our data continuously in memory, and make sure that we access it linearly as much as possible.
This happens to be the basic concept of Entity Component Systems.
