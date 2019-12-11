using Unity.Entities ;
using UnityEngine ;


namespace Antypodish.ECS
{
    
    public struct IsActiveTag : IComponentData {}

    public struct RayData : IComponentData 
    {
        public Ray ray ;
    }

    public struct BoundsData : IComponentData 
    {
        public Bounds bounds ;
    }
    
    public struct RayMaxDistanceData : IComponentData 
    {
        public float f ;
    }
}
