using Unity.Entities ;
using Unity.Mathematics ;
using Unity.Collections ;

using UnityEngine;

namespace ECS.Octree
{

    class OctreeExample_GetCollidingBoundsInstancesBarrier_Bounds2Octree : BarrierSystem { }
  
    class OctreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree : JobComponentSystem
    {

        [Inject] private OctreeExample_GetCollidingBoundsInstancesBarrier_Bounds2Octree barrier ;
              
        protected override void OnCreateManager ( )
        {
            base.OnCreateManager ( );

            

            // Test bounds
            // Many bounds, to octree pair
            // Where each bounds entty has one octree entity target.


            // Toggle manually only one example systems at the time
            if ( true ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Bounds Instances System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            EntityCommandBuffer ecb = barrier.CreateCommandBuffer () ;
            Entity newOctreeEntity = EntityManager.CreateEntity ( ) ;

            AddNewOctreeSystem._CreateNewOctree ( ecb, newOctreeEntity, 8, float3.zero, 1, 1, 1 ) ;









            // ***** Example Components To Add / Remove Incstance ***** //
            
            // Request to add 100 instances
            // User is responsible to ensure, that instances IDs are unique in the octrtree.    
            
            Entity newBlockEntity = new Entity () ; // empty entity
            NativeArray <Entity> a_instanceEntities = new NativeArray <Entity> ( 100, Allocator.Temp ) ;
            int i_instanceEntityIndex = 0 ;
            
            for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
            {  
                newBlockEntity = EntityManager.CreateEntity () ;
                a_instanceEntities [i_instanceID] = newBlockEntity ;                  
                // i_instanceEntityIndex ++ ;
            }
            

            EntityManager.AddBuffer <AddInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.     
            BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;
            DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement = addInstanceBufferElement [newOctreeEntity] ;  

            for ( int i_instanceID = 0; i_instanceID < 100; i_instanceID ++ )
            {  
                
                newBlockEntity = a_instanceEntities [i_instanceEntityIndex] ;
                i_instanceEntityIndex ++ ;
                
                int x = i_instanceID % 10 ;
                int y = (int) math.floor ( i_instanceID / 10 ) ;
                float3 f3_blockCenter = new float3 ( x, 0, y ) + new float3 ( 1, 1, 1 )  * 0.5f ;

                Blocks.Methods._AddBlockRequestViaCustomBufferWithEntity ( ecb, newBlockEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 1 ) ;


                Debug.Log ( "Test instance spawn #" + i_instanceID + " x: " + x + " y: " + y ) ;

                Bounds bounds = new Bounds () { center = f3_blockCenter, size = Vector3.one * 1 } ;
                
                AddInstanceBufferElement addInstanceBuffer = new AddInstanceBufferElement () 
                {
                    i_instanceID = newBlockEntity.Index,
                    i_version = newBlockEntity.Version,
                    // i_instanceID = i_instanceID,
                    instanceBounds = bounds
                };

                a_addInstanceBufferElement.Add ( addInstanceBuffer ) ;
            }



            // Request to remove 53 instances.
            // User is responsible to ensure, that requested instance ID to delete exists in the octree.            
            EntityManager.AddBuffer <RemoveInstanceBufferElement> ( newOctreeEntity ) ; // Once system executed and instances were removed, tag will be deleted.

            BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
            DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement = removeInstanceBufferElement [newOctreeEntity] ;  
            
            i_instanceEntityIndex = 0 ;
            for ( int i_instanceID = 0; i_instanceID < 53; i_instanceID ++ )
            {          
                
                // i_instanceEntityIndex -- ;
                Entity removeEntity = a_instanceEntities [i_instanceEntityIndex] ;                 
                i_instanceEntityIndex ++ ;


                int x = i_instanceID % 10 ;
                int y = (int) math.floor ( i_instanceID / 10 ) ;
                Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y ) ;
                                         
                RemoveInstanceBufferElement removeInstanceBuffer = new RemoveInstanceBufferElement () 
                {
                    i_instanceID = removeEntity.Index
                    // i_instanceID = i_instanceID
                } ;
                
                a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;
             
                

                
                Blocks.Methods._RemoveBlockRequestWithEntity ( ecb, removeEntity ) ;

            }


            a_instanceEntities.Dispose () ;



            // ***** Example Bounds Components For Collision Checks ***** //

            // Create test bounds
            // Many bounds, to single or many octrees
            // Where each bounds has one entity target.
            for ( int i = 0; i < 10; i ++ ) 
            {
                ecb.CreateEntity ( ) ; // Check bounds collision with octree and return colliding instances.                
                ecb.AddComponent ( new IsActiveTag () ) ; 
                ecb.AddComponent ( new GetCollidingBoundsInstancesTag () ) ;  
                ecb.AddComponent ( new BoundsData ()
                {
                bounds = new Bounds () { center = float3.zero, size = new float3 ( 5, 5, 5 ) }
                } ) ; 
                // Check bounds collision with octree and return colliding instances.
                ecb.AddComponent ( new OctreeEntityPair4CollisionData () 
                {
                    octree2CheckEntity = newOctreeEntity
                } ) ;
                ecb.AddComponent ( new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                ecb.AddBuffer <CollisionInstancesBufferElement> () ;
            } // for
                                     
        }
        
    }
}


