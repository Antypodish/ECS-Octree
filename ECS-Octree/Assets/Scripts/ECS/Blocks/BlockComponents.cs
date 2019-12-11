using Unity.Mathematics ;
using Unity.Entities ;


namespace Antypodish.ECS.Blocks
{   
    
    /*
    public struct AddBlockData : IComponentData 
    {    
        public float3 f3_position ; 
        
        public float3 f3_scale ;

        public float4 f4_color ;
    }
    */
        
    public struct AddBlockTag : IComponentData {}  
    
    public struct RemoveBlockTag : IComponentData {}    
    
}
