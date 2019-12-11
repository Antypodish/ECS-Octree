using Unity.Collections ;
using Unity.Entities ;
using Unity.Mathematics ;
using UnityEngine ;


namespace Antypodish.ECS.Octree.Examples
{
    // ***** Example Components To Add / Remove Instances ***** //

    // This method are just examples. They may require extract relevant part of the code, to move into your system / application.

    internal class Common
    {


        static public NativeArray <Entity> _CreateInstencesArray ( EntityManager entityManager, int i_instances2AddCount )
        {
            
            NativeArray <Entity> a_instanceEntities = new NativeArray <Entity> ( i_instances2AddCount, Allocator.Temp ) ;

            for ( int i_instanceID = 0; i_instanceID < i_instances2AddCount; i_instanceID ++ )
            {  
                Entity newBlockEntity = entityManager.CreateEntity () ;
                a_instanceEntities [i_instanceID] = newBlockEntity ;    
            }

            return a_instanceEntities ;
        }



        // Request to add some instances.
        // User is responsible to ensure, that instances IDs are unique in the octrtree.        
        static public void _RequesAddInstances ( EntityCommandBuffer ecb, Entity octreeEntity, BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement, ref NativeArray <Entity> a_instanceEntities, int i_instances2AddCount )
        {

            DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement = addInstanceBufferElement [octreeEntity] ;  

            int i_instanceEntityIndex = 0 ;

            for ( int i_instanceID = 0; i_instanceID < i_instances2AddCount; i_instanceID ++ )
            {  

                Entity newBlockEntity = a_instanceEntities [i_instanceEntityIndex] ;
                i_instanceEntityIndex ++ ;
                
                int x = i_instanceID % 1000 ;
                int y = i_instanceID % 100 ;
                int z = (int) math.floor ( i_instanceID / 1000 ) ;
                float3 f3_blockCenter = new float3 ( x, y, z ) + new float3 ( 1, 1, 1 )  * 0.5f ;

                ...
                _AddBlockRequestViaCustomBufferWithEntity ( ref EntityCommandBuffer.Concurrent ecb, int jobIndex, Entity blockEntity, float3 f3_position, float3 f3_scale, MeshType meshType, [ReadOnly] ref Bootstrap.EntitiesPrefabs entitiesPrefabs, ref Bootstrap.RenderMeshTypes renderMeshTypes )
                Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, newBlockEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 1 ) ;



                // Bounds of instance node, 
                // hence entity block as well.
                Bounds bounds = new Bounds () { center = f3_blockCenter, size = new float3 ( 1, 1, 1 ) * 1 } ;
                
                AddInstanceBufferElement addInstanceBuffer = new AddInstanceBufferElement () 
                {
                    i_instanceID = newBlockEntity.Index,
                    i_version = newBlockEntity.Version,
                    instanceBounds = bounds
                };

                a_addInstanceBufferElement.Add ( addInstanceBuffer ) ;
            }

        }


        /// <summary>
        /// Request to remove some instances.
        /// User is responsible to ensure, that requested instance ID to delete exists in the octree.  
        /// </summary>
        static public void _RequestRemoveInstances ( EntityCommandBuffer ecb, Entity octreeEntity, BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement, ref NativeArray <Entity> a_instanceEntities, int i_instances2RemoveCount )
        {

            DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement = removeInstanceBufferElement [octreeEntity] ;  
            
            int i_instanceEntityIndex = 0 ;

            // Request to remove 53 instances.
            for ( int i_instanceID = 0; i_instanceID < i_instances2RemoveCount; i_instanceID ++ )
            {            
                
                Entity removeEntity = a_instanceEntities [i_instanceEntityIndex] ;                 
                i_instanceEntityIndex ++ ;

                /*
                int x = i_instanceID % 50 ;
                int y = i_instanceID % 5 ;
                int z = Mathf.FloorToInt ( i_instanceID / 50 ) ;
                Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y + " z: " + z ) ;
*/
                                         
                RemoveInstanceBufferElement removeInstanceBuffer = new RemoveInstanceBufferElement () 
                {
                    i_instanceID = removeEntity.Index,                    
                };
                
                a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;
                
                // Actual entity block removal
                Blocks.PublicMethods._RemoveBlockRequestWithEntity ( ecb, removeEntity ) ;

            }

        }


    }
}
