using System;
using System.Collections.Generic;
using Unity.Collections ;
using Unity.Entities ;
using Unity.Jobs ;
using Unity.Mathematics ;
using UnityEngine;
using Unity.Rendering ;
using Unity.Burst ;

namespace ECS.Octree
{

  
    class OctreeTestSystem : JobComponentSystem
    {

        [Inject] private AddNewOctreeBarrier barrier ;
        
        EntityArchetype octreeArchetype ;        
        ComponentGroup group ;

        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );


            // Test rays
            // Many rays, to many octrees
            // Where each ray has one entity target.


            // Toggle manually only one example systems at the time
            if ( true ) return ; // Early exit


            Debug.Log ( "Start Test Octree System" ) ;


            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

            AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;




            // ***** Instance Optional Components ***** //
            
            // Request to add 100 instances
            // User is responsible to ensure, that instances IDs are unique in the octrtree.
            EntityManager.AddBuffer <AddInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.         

            BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;
            DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement = addInstanceBufferElement [newOctreeEntity] ;  

            for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
            {  

                int x = i_instanceID % 10 ;
                int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;

                Debug.Log ( "Test instance spawn #" + i_instanceID + " x: " + x + " y: " + y ) ;

                Bounds bounds = new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ;
                
                AddInstanceBufferElement addInstanceBuffer = new AddInstanceBufferElement () 
                {
                    i_instanceID = i_instanceID,
                    instanceBounds = bounds
                };

                a_addInstanceBufferElement.Add ( addInstanceBuffer ) ;
            }



            // Request to remove 53 instances.
            // User is responsible to ensure, that requested instance ID to delete exists in the octree.            
            EntityManager.AddBuffer <RemoveInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were removed, tag will be deleted.

            BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
            DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement = removeInstanceBufferElement [newOctreeEntity] ;  
            
            for ( int i_instanceID = 0; i_instanceID < 53; i_instanceID ++ )
            {            
                int x = i_instanceID % 10 ;
                int y = Mathf.FloorToInt ( i_instanceID / 10 ) ;
                Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y ) ;
                                         
                RemoveInstanceBufferElement removeInstanceBuffer = new RemoveInstanceBufferElement () 
                {
                    i_instanceID = i_instanceID,                    
                };
                
                a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;

            }




            // Create test rays
            // Many rays, to many octrees
            // Where each ray has one entity target.
            for ( int i = 0; i < 1000; i ++ ) 
            {
                ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                
                ecb.AddComponent ( new IsActiveTag () ) ; 
                ecb.AddComponent ( new RayData () ) ; 
                ecb.AddComponent ( new RayMaxDistanceData ()
                {
                    f = 100f
                } ) ; 
                // Check bounds collision with octree and return colliding instances.
                ecb.AddComponent ( new OctreeEntityPair4CollisionData () 
                {
                    octree2CheckEntity = newOctreeEntity
                } ) ;
                ecb.AddComponent ( new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                ecb.AddBuffer <CollisionInstancesBufferElement> () ;
            } // for


            
            

            // ***** Collision Detection Components, Check List ***** //

// TODO: replace instance ID with entity?
            // ecb.AddComponent ( newOctreeEntity, new IsBoundsCollidingTag () ) ; // Check boundary collision with octree instances.
            // ecb.AddComponent ( newOctreeEntity, new IsRayCollidingTag () ) ; // Check ray collision with octree instances.
            // ecb.AddComponent ( newOctreeEntity, new GetCollidingBoundsInstancesTag () ) ; // Check bounds collision with octree and return colliding instances.
            // ecb.AddComponent ( newOctreeEntity, new GetCollidingRayInstancesTag () ) ; // Check bounds collision with octree and return colliding instances.

// TODO: incomplete Get max bounds
            // ecb.AddComponent ( newOctreeEntity, new GetMaxBoundsTag () ) ;

                         
        }
        
    }
}


