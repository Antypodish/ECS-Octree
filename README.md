# ECS-Octree

![ECS Octree Example](https://forum.unity.com/attachments/upload_2019-1-9_8-21-26-png.355738/)

[![Watch the video](https://forum.unity.com/attachments/upload_2019-1-9_8-21-26-png.355738/)](https://www.youtube.com/watch?v=OlGUWbtUBsM)

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
* Comply with Burst. (mostly done now)
* ~~Replace collision check List, with BufferArray.~~
* ~~Convert GameObjects into ECS mesh renderings.~~
* ~~Implement multithreading into systems, for multiple parallel octrees checks.~~
* Convert some integers to bytes, and uInt, where applicable.


### Links
Main Unity forum thread for this project.
[WIP] Octree in Pure ECS : BufferArray based with source code
https://forum.unity.com/threads/wip-octree-in-pure-ecs-bufferarray-based-with-source-code.546240/#post-4071661

#### Conversion from Unity 2018.3b to 2020.1a (Update 2019 December 11)

Upgrade form
* Entitities 0.0.12 p 21 to 0.2.0 p 18
* Burst 0.2.4 p37 to 1.2.0 p 10
* Collections 0.0.9 to 0.2.0 p 13
* Jobs 0.0.7 to 0.2.1 p 3
* Mathematics 0.0.12 p 19 to 1.1.0

Added:
* Hybrid Renderer 0.2.0
* "Antypodish" to namespace.

#### Ray Highlight implemented  (Update 2019 January 09)

Most relevant systems are burst complaint. There may be still a bit room to work on that.
Implemented ray, highlighting blocks, for confirming working raycast.
There is in total 8 example systems. 4 for Ray-Octree, and 4 for Bounds-Octree.
Where 2 of each are checking only if is colliding, and other two of each, returns list of colliding instances.
In case of Ray-Octree examples, nearest collision instance is returned, along with its ID and distance.
One example should be run at the time. Multiple example at the same time were not tested.

Instances now hold additional information, allowing to store either single ID, which must be unique per tree, or using this ID as Entity index, with conjunction of Entity version. Otherwise version can be ignored.

TODO:
* Optimize examples

#### Rays - Octrees in parallel (Update 2019 January 07)

Big code clean up.
Removed debugging commented out references, to linearized form.

Implemented two Rays to Octree and Octrees to Ray multithreaded collision checks systems.
Ray to Octree system checks, for collision between many rays and one or more octrees.
Octree to Ray system checks, for collision between many octrees and one or more rays.
Octree Entity to Ray Entity and Ray Entity to Octree Entity can be paired, for relevant checks.
Two example systems with OnCreate () were added, which allows run selected method.

Note:
These systems has disabled main debugging atm.

TODO:
* ~~Bounds octree-instance collision checks systems on multithreading.~~
