using UnityEngine ;
using Unity.Entities ;

namespace ECS
{
    
    public struct IsActiveTag : IComponentData {}

    public struct RayData : IComponentData 
    {
        public Ray ray ;
    }
    
    public struct RayMaxDistanceData : IComponentData 
    {
        public float f ;
    }
}
