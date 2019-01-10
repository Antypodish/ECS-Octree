using Unity.Mathematics ;
using Unity.Entities ;


namespace ECS.Blocks
{   

    public class PublicMethods
    {
        
        /// <summary>
        /// Requests to adds new entity with block component.
        /// Call it from whatever place
        /// </summary>
        static public EntityCommandBuffer _AddBlockRequestViaCustomBuffer ( EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale )
        // static public EntityCommandBuffer _AddBlockRequestViaCustomBuffer ( EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale, float3 f3_directionAxisOfCreation, Entity entitySrc, float4 f4_color )
        {
            //Debug.Log ( "Requested add new Block from entity # " + entitySrc.Index + "; at postion " + f3_position ) ;

            ecb.CreateEntity ( ) ;            
            ecb.AddComponent ( new AddBlockData { 
//                referenceNeighbourBlock = entitySrc, 
                f3_position = f3_position, f3_scale = f3_scale, 
 //               f_directionFromReferenceNeighbourBlock = f3_directionAxisOfCreation, 
 //               f4_color = f4_color 
            } ) ; // tag it as new block. This tag will be removed after block added            

            return ecb ;
        }

        /// <summary>
        /// Requests to adds new entity with block component.
        /// Requires created entity with command buffer, before calling this method.
        /// Call it from whatever place
        /// </summary>
        static public EntityCommandBuffer _AddBlockRequestViaCustomBufferNoNewEntity ( EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale )
        // static public EntityCommandBuffer _AddBlockRequestViaCustomBufferNoNewEntity ( EntityCommandBuffer ecb, float3 f3_position, float3 f3_scale, float3 f3_directionAxisOfCreation, Entity entitySrc, float4 f4_color )
        {
            //Debug.Log ( "Requested add new Block from entity # " + entitySrc.Index + "; at postion " + f3_position ) ;
         
            
            ecb.AddComponent ( new AddBlockData { 
//                referenceNeighbourBlock = entitySrc, 
                f3_position = f3_position, 
                f3_scale = f3_scale, 
 //               f_directionFromReferenceNeighbourBlock = f3_directionAxisOfCreation, 
//                f4_color = f4_color 
            } ) ; // tag it as new block. This tag will be removed after block added            
            
            return ecb ;
        }

        /// <summary>
        /// Requests to adds new entity with block component.
        /// Call it from whatever place
        /// </summary>
        static public EntityCommandBuffer _AddBlockRequestViaCustomBufferWithEntity ( EntityCommandBuffer ecb, Entity entity, float3 f3_position, float3 f3_scale )
        // static public EntityCommandBuffer _AddBlockRequestViaCustomBufferWithEntity ( EntityCommandBuffer ecb, Entity entity, float3 f3_position, float3 f3_scale, float3 f3_directionAxisOfCreation, Entity entitySrc, float4 f4_color )
        {
            //Debug.Log ( "Requested add new Block #" + blockEntity.Index + " from entity # " + entitySrc.Index + "; at postion " + f3_position ) ;
                          
            UnityEngine.Debug.LogWarning ( entity ) ;

            ecb.AddComponent ( entity, new AddBlockData { 
 //               referenceNeighbourBlock = entitySrc, 
                f3_position = f3_position, 
                f3_scale = f3_scale, 
//                f_directionFromReferenceNeighbourBlock = f3_directionAxisOfCreation, 
//                f4_color = f4_color 
            } ) ; // tag it as new block. This tag will be removed after block added            

            return ecb ;
        }




        
        /// <summary>
        /// Requests to remove entity block with.
        /// Call it from whatever place
        /// </summary>
        static public void _RemoveBlockRequestWithEntity ( EntityCommandBuffer ecb, Entity entity )
        {
            ecb.AddComponent ( entity, new RemoveBlockTag () ) ; // tag it as block to remove.

            // Debug.Log ( "Requested to remove Block #" + entity.Index ) ;
        }

        /// <summary>
        /// Requests to remove entity block with.
        /// Call it from whatever place
        /// </summary>
        static public void _RemoveBlockRequest ( EntityCommandBuffer ecb )
        {
            ecb.AddComponent ( new RemoveBlockTag () ) ; // tag it as block to remove.

            // Debug.Log ( "Requested to remove Block #" + entity.Index ) ;
        }

    }

}
