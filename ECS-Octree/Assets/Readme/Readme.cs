// A Dynamic, Loose Octree for storing any objects that can be described with AABB bounds
// See also: PointOctree, where objects are stored as single points and some code can be simplified
// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes)
//			and places objects into the appropriate nodes. This allows fast access to objects
//			in an area of interest without having to check every object.
// Dynamic: The octree grows or shrinks as required when objects as added or removed
//			It also splits and merges nodes as appropriate. There is no maximum depth.
//			Nodes have a constant - numObjectsAllowed - which sets the amount of items allowed in a node before it splits.
// Loose:	The octree's nodes can be larger than 1/2 their parent's length and width, so they overlap to some extent.
//			This can alleviate the problem of even tiny objects ending up in large nodes if they're near boundaries.
//			A looseness value of 1.0 will make it a "normal" octree.

// Code based on https://github.com/Unity-Technologies/UnityOctree
// It can be found many similarities in terms of structure and comments.
// However, this code has been converted into linearized form,
// initially to List representation, then further to be more appropriate for into ECS conversion.

// See OctreeExample_Selector.cs for details, to select examples.

// By Dobromil K Duda

// 2019 January 10
// Debug optimisation, and duplicate code reduction
// See OctreeExample_Selector.cs for details, to select examples.

// 2019 January 08
// Most relevant octree check systems, are nonw in parallel and burst optimised.
// Examples files added.
// Ray(cast) block highlight support added.


// 2019 January 06
// Working collision checks in pseudo ECS.
// Implementing systems, converting Vectors3 into floats3
// 
// TODO: 
// Bounds to check
// Comply with Burst
// Replace collision check List, with BufferArray
// Convert GameObjects into ECS mesh renderings

// 2019 January 05
// Linearized into list representation.

// 2019 January 03
// Approx. initiated project
// Using UnityOctree asset
// Starting coversion into linearized form, of Lists, from object oriented form.

