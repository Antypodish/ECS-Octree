# ECS-Octree
This Unity project is inspired by Unity-Technologies/UnityOctree github repository (https://github.com/Unity-Technologies/UnityOctree) which is Classic OOP based octree system.
In this Unity project, aim is to develop pure, or Hybrid ECS based octree system, for which each octree, is represented by own entity.
Octree nodes, are stored as BufferArrays.

ECS scripts are located in Scripts/ECS/Octree folder.
There is also OOP-Testing directory, which contains Linearization folder, with intermidiate step, just for testing.
And Originals folder, which contain initial project, from which this project is being derived.

ECS Octree Directory, contains AddRemove, CollisionChecks and Core folders.
AddRemove folder contains ECS systems, which allows to initialize Octree as entity, which stores data as components and BufferArrays.
There is also add and remove instances systems, which manages octree node behind scene.
Collision checks folder, contains systems, which can be triggered by relevant tags, to check, if ray intersaction, or bounds overlap ocures.
Core folder, contains general octree files. There is Octreecomponents.cs file, which may be of interest.

Adding and removing instances, expnads and shrinks octree, and its nodes.
Added instances, return unique ID, if for example raycast collision checks is conducted.
When adding new instance, it is repsonsibility of user, to ensure, that ID do not exists in octree.
Further instance ID, can be representing Entity index. It will require additional pairing with entity version, to construct actual entity, if that what is required.

Work in progress.

TODO: 
* Bounds vectors-floats to check.
* * Comply with Burst.
* Replace collision check List, with BufferArray.
* Convert GameObjects into ECS mesh renderings.
* Implement multithreading into systems, for multiple parallel octrees checks.
* Convert some integers to bytes, and uInt, where applicable.


#### Rays - Octrees in parallel (Update 2019 January 07)

Implemented two Rays to Octree and Octrees to Ray multithreaded collision checks systems.
Ray to Octree system checks, for collision between many rays and one or more octrees.
Octree to Ray system checks, for collision between many octrees and one or more rays.
Octree Entity to Ray Entity and Ray Entity to Octree Entity can be paired, for relevant checks.
Two example systems with OnCreate () were added, which allows run selected method.

Note
These systems has disbaled main debugging atm.

TODO:
* Bounds octree-instance collision checks systems on multithreading.
