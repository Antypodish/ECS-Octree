

            // ***** Collision Detection Components, Check List ***** //

// TODO: replace instance ID with entity?
            // ecb.AddComponent ( newOctreeEntity, new IsBoundsCollidingTag () ) ; // Check boundary collision with octree instances.
            // ecb.AddComponent ( newOctreeEntity, new GetCollidingBoundsInstancesTag () ) ; // Check bounds collision with octree and return colliding instances.

// TODO: incomplete Get max bounds
            // ecb.AddComponent ( newOctreeEntity, new GetMaxBoundsTag () ) ;
/*

    Comply with Burst.
    Replace collision check List, with BufferArray.
    Bounds vectors-floats to check.
    Convert GameObjects into ECS mesh renderings.
    Implement multithreading into systems, for multiple parallel octrees checks.
    Convert some integers to bytes, and uInt, where applicable.

    Bounds octree-instance collision checks systems on multithreading.


    */