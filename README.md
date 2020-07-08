![Banner](https://auroriax.com/wp-content/uploads/2020/03/code-sample-banner.png)

# Pooling system code sample by Tom Hermans
This repo contains a C# Unity 2019.2 code sample, demonstrating a simple instance pooling system. A pool will instance a number of instances from the indicated prefab, which other game objects can (de)activate. This way, the game does not need to instantiate new objects frequently, which makes the game more optimized.

## [InstancePool.cs](https://github.com/Auroriax/code-sample/blob/master/Assets/InstancePool.cs)
This class manages everything related to the pooling system. In the inspector, you can set the max size of the pool, which is how many instances it will preload in Play mode. Other GameObject scripts can then request the pool to deactivate one of its objects at the indicated position.  Other scripts can then request a pool instance to be (de-)activated. 

There are also settings for should behave when the pool limit is reached: instance new elements up to an absolute maximum, and/or recycle the oldest elements when the limit is reached. The script also exposes some UnityEvents in the inspector: in this sample, it's used to update an UI label.

## [PlayerController.cs](https://github.com/Auroriax/code-sample/blob/master/Assets/PlayerController.cs)
This allows the player to move, and it demonstrates how a script can interface with the instance pool. There are some settings for movement speed and quality of collision checks (in how many chunks the movement raycasts will be split up).

The player will plant a flower at its feet each frame, and will apply a random rotation to it. You can also snip the flowers around you by pressing [Space].

### More info
Made by [Tom Hermans](https://auroriax.com).
TextMeshPro is an (included) dependency for drawing the UI.
