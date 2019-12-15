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

            NativeArray <Entity> na_instanceEntities = new NativeArray <Entity> ( i_instances2AddCount, Allocator.Temp ) ;
            
            entityManager.Instantiate ( PrefabsSpawner_FromEntity.spawnerEntitiesPrefabs.prefab01Entity, na_instanceEntities ) ;
            // entityManager.CreateEntity ( BlocksArchetypes.blockArchetype, a_instanceEntities ) ;
            // entityManager.Instantiate ( entitiesPrefabs.blockEntity, a_instanceEntities ) ;
            /*
            NativeArray <Entity> a_instanceEntities = new NativeArray <Entity> ( i_instances2AddCount, Allocator.Temp ) ;
            
            Entity newBlockEntity = 
            for ( int i_instanceID = 0; i_instanceID < i_instances2AddCount; i_instanceID ++ )
            {  
                a_instanceEntities [i_instanceID] = newBlockEntity ;    
            }
            */

            return na_instanceEntities ;
        }



        // Request to add some instances.
        // User is responsible to ensure, that instances IDs are unique in the octrtree.        
        static public void _RequesAddInstances ( ref EntityCommandBuffer ecb, Entity octreeEntity, BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement, ref NativeArray <Entity> na_instanceEntities, int i_instances2AddCount )
        {

            DynamicBuffer <AddInstanceBufferElement> a_addInstanceBufferElement = addInstanceBufferElement [octreeEntity] ;  
            a_addInstanceBufferElement.ResizeUninitialized ( i_instances2AddCount ) ; // Set required capacity.

            int i_instanceEntityIndex = 0 ;

            for ( int i_instanceID = 0; i_instanceID < i_instances2AddCount; i_instanceID ++ )
            {  

                Entity newBlockEntity = na_instanceEntities [i_instanceEntityIndex] ;
                i_instanceEntityIndex ++ ;
                
                // Formation A
                /*
                int x = i_instanceID % 1000 ;
                int y = i_instanceID % 100 ;
                int z = (int) math.floor ( i_instanceID / 1000 ) ;
                */ 

                // Formation B
                int z = (int) math.floor ( i_instanceID / 625 ) ; // 25*25 = 625
                int y = (int) math.floor ( i_instanceID / 25 ) - z * 25 ;
                int x = i_instanceID % 25 ;

                float3 f3_blockCenter = new float3 ( x, y, z ) + new float3 ( 1, 1, 1 ) * 0.5f ;

                // For rendering.
                Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ref ecb, newBlockEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 0.90f, MeshType.Prefab01 ) ;

                // Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, newBlockEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 1 ) ;



                // Bounds of instance node, 
                // hence entity block as well.
                Bounds bounds = new Bounds () { center = f3_blockCenter, size = new float3 ( 1, 1, 1 ) * 1 } ;
                
                a_addInstanceBufferElement [i_instanceID] = new AddInstanceBufferElement () 
                {
                    i_instanceID   = newBlockEntity.Index,
                    i_version      = newBlockEntity.Version,
                    instanceBounds = bounds
                };

                // a_addInstanceBufferElement.Add ( addInstanceBuffer ) ;
            }

        }


        /// <summary>
        /// Request to remove some instances.
        /// User is responsible to ensure, that requested instance ID to delete exists in the octree.  
        /// </summary>
        static public void _RequestRemoveInstances ( ref EntityCommandBuffer ecb, Entity octreeEntity, BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement, ref NativeArray <Entity> na_instanceEntities, int i_instances2RemoveCount )
        {

            DynamicBuffer <RemoveInstanceBufferElement> a_removeInstanceBufferElement = removeInstanceBufferElement [octreeEntity] ;  
            a_removeInstanceBufferElement.ResizeUninitialized ( i_instances2RemoveCount ) ; // Set required capacity.

            int i_instanceEntityIndex = 0 ;

            // Request to remove 53 instances.
            for ( int i_instanceID = 0; i_instanceID < i_instances2RemoveCount; i_instanceID ++ )
            {            
                
                Entity removeEntity = na_instanceEntities [i_instanceEntityIndex] ;                 
                i_instanceEntityIndex ++ ;

                /*
                int x = i_instanceID % 50 ;
                int y = i_instanceID % 5 ;
                int z = Mathf.FloorToInt ( i_instanceID / 50 ) ;
                Debug.Log ( "Test instance remove #" + i_instanceID + " x: " + x + " y: " + y + " z: " + z ) ;
*/
                                         
                a_removeInstanceBufferElement [i_instanceID] = new RemoveInstanceBufferElement () { i_instanceID = removeEntity.Index } ;

                // a_removeInstanceBufferElement.Add ( removeInstanceBuffer ) ;
                
                // Actual entity block removal
                Blocks.PublicMethods._RemoveBlockRequestWithEntity ( ref ecb, removeEntity ) ;

            }

        }


    }
}
