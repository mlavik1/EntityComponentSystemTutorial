# UnityBalls sample

Simulates the movement of a number of balls inside of a box.
The bals have a random position, speed and direction, and will bounce off the walls.

## How to run the project

The project was made with Unity 2020.3.33f1, but other versions should work as well.

Simply open the SampleScene and click play.

You can change the ball count in the BallManager game object.

## Performance issues

- We iterate over a list of Ball components to update the balls.
These are basically pointers to data stored elsewhere in memory, which results in poor cache efficiency.
When updating the positions of the balls we modify `Ball.transform` which also is a reference to data stored another place in memory.
- We block the main thread while updating the balls (would be nice to do this async!).
- We use 3 if-checks in the code for making the balls bounce off the walls.
This can in fact be vectorized optimised with [SIDM](https://en.wikipedia.org/wiki/Single_instruction,_multiple_data) operations.
See the 4th sample project for how to do this.
