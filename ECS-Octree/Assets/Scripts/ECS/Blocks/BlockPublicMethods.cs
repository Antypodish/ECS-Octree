using Unity.Mathematics ;
using Unity.Collections ;
using Unity.Transforms ;
using Unity.Rendering ;
using Unity.Entities ;


namespace Antypodish.ECS.Blocks
{   

    public class PublicMethods
    {
        /// <summary>
        /// Requests to adds new entity with block component.
        /// Call it from whatever place
        /// </summary>
        static public void _AddBlockRequestViaCustomBufferWithEntity ( ref EntityCommandBuffer ecb, Entity blockEntity, float3 f3_position, float3 f3_scale, MeshType meshType, [ReadOnly] ref Bootstrap.RenderMeshTypes renderMeshTypes )
        {
            //Debug.Log ( "Requested add new Block #" + blockEntity.Index + " from entity # " + entitySrc.Index + "; at postion " + f3_position ) ;
                          
//             UnityEngine.Debug.LogWarning ( entity ) ;
            
            ecb.SetComponent ( blockEntity, new MeshTypeData { type = meshType } ) ;
            
            ecb.SetComponent ( blockEntity, new Translation { Value = f3_position } ) ;
            ecb.SetComponent ( blockEntity, new Rotation { Value = quaternion.identity } ) ;
            ecb.SetComponent ( blockEntity, new NonUniformScale { Value = f3_scale } ) ;
                    
            RenderMesh renderer = Bootstrap._SelectRenderMesh ( meshType, ref renderMeshTypes ) ;
            ecb.SetSharedComponent ( blockEntity, renderer ) ;
            //    ... keep eye on this tag!
            // ecb.AddComponent <AddBlockTag> ( blockEntity ) ; // Block added. Remove tag

        }


        /// <summary>
        /// Requests to remove entity block with.
        /// Call it from whatever place
        /// </summary>
        static public void _RemoveBlockRequestWithEntity ( ref EntityCommandBuffer ecb, Entity blockEntity )
        {
            // ecb.AddComponent ( blockEntity, new RemoveBlockTag () ) ; // tag it as block to remove.
            ecb.DestroyEntity ( blockEntity ) ;

            // Debug.Log ( "Requested to remove Block #" + entity.Index ) ;
        }

        /*
        /// <summary>
        /// Requests to adds new entity with block component.
        /// Call it from whatever place
        /// </summary>
        static public void _AddBlockRequestViaCustomBuffer ( ref EntityCommandBuffer.Concurrent ecb, int jobIndex, float3 f3_position, float3 f3_scale, MeshType meshType, [ReadOnly] ref Bootstrap.EntitiesPrefabs entitiesPrefabs, ref Bootstrap.RenderMeshTypes renderMeshTypes )
        // static public EntityCommandBuffer _AddBlockRequestViaCustomBuffer ( EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale, float3 f3_directionAxisOfCreation, Entity entitySrc, float4 f4_color )
        {
            //Debug.Log ( "Requested add new Block from entity # " + entitySrc.Index + "; at postion " + f3_position ) ;

            Entity blockEntity = ecb.Instantiate ( jobIndex, entitiesPrefabs.blockEntity ) ;         
            

            ecb.SetComponent ( jobIndex, blockEntity, new MeshTypeData { type = meshType } ) ;
            
            ecb.SetComponent ( jobIndex, blockEntity, new Translation { Value = f3_position } ) ;
            ecb.SetComponent ( jobIndex, blockEntity, new Rotation { Value = quaternion.identity } ) ;
            ecb.SetComponent ( jobIndex, blockEntity, new NonUniformScale { Value = f3_scale } ) ;
                    
            RenderMesh renderer = Bootstrap._SelectRenderMesh ( meshType, ref renderMeshTypes ) ;
            ecb.SetSharedComponent ( jobIndex, blockEntity, renderer ) ;
                
            ecb.RemoveComponent <AddBlockTag> ( jobIndex, blockEntity ) ; // Block added. Remove tag

        }

        /// <summary>
        /// Requests to adds new entity with block component.
        /// Requires created entity with command buffer, before calling this method.
        /// Call it from whatever place
        /// </summary>
        static public EntityCommandBuffer _AddBlockRequestViaCustomBufferNoNewEntity ( ref EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale )
        // static public EntityCommandBuffer _AddBlockRequestViaCustomBufferNoNewEntity ( EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale, float3 f3_directionAxisOfCreation, Entity entitySrc, float4 f4_color )
        {
            //Debug.Log ( "Requested add new Block from entity # " + entitySrc.Index + "; at postion " + f3_position ) ;
         
            AddBlockTag
            ecb.AddComponent ( new AddBlockData { 
//                referenceNeighbourBlock = entitySrc, 
                f3_position = f3_position, 
                f3_scale = f3_scale, 
 //               f_directionFromReferenceNeighbourBlock = f3_directionAxisOfCreation, 
//                f4_color = f4_color 
            } ) ; // tag it as new block. This tag will be removed after block added            
            
            ecb.SetComponent ( jobIndex, blockEntity, new Translation { Value = addBlockData.f3_position } ) ;
            ecb.SetComponent ( jobIndex, blockEntity, new Rotation { Value = quaternion.identity} ) ;
            ecb.SetComponent ( jobIndex, blockEntity, new NonUniformScale { Value = addBlockData.f3_scale } ) ;
            // ...
            // ecb.AddComponent ( jobIndex, blockEntity, new Highlight.MeshType { type = Highlight.Common.MeshType.Default } ) ;

                    
            RenderMesh renderer = Bootstrap._SelectRenderMesh ( meshType.type, ref renderMeshTypes ) ;
            ecb.SetSharedComponent ( jobIndex, blockEntity, renderer ) ;
                
            ecb.RemoveComponent <AddBlockTag> ( jobIndex, blockEntity ) ; // Block added. Remove tag

            return ecb ;
        }
        */

        

        /*
        /// <summary>
        /// Requests to remove entity block with.
        /// Call it from whatever place
        /// </summary>
        static public void _RemoveBlockRequest ( EntityCommandBuffer ecb )
        {
            ecb.AddComponent ( new RemoveBlockTag () ) ; // tag it as block to remove.

            // Debug.Log ( "Requested to remove Block #" + entity.Index ) ;
        }
        */

    }

}
