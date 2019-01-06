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
Bounds vectors to check
Comply with Burst
Replace collision check List, with BufferArray
Convert GameObjects into ECS mesh renderings
Implement multithreading into systems, for multiple parallel octrees checks.
Convert some integers to bytes, and uInt, where applicable.
