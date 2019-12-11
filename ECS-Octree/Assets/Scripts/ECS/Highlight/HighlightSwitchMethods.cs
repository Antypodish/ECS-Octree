using Unity.Entities ;

// TODO : Validate entity, if exists

namespace Antypodish.ECS.Highlight
{

    public class SwitchMethods
    {     
        
        static private Entity previousHighlightedEntity ;

        static public void _Initialize ( )
        {
            previousHighlightedEntity = new Entity () 
            { 
                Index = 0, 
                Version = 0   
            } ;

            UnityEngine.Debug.LogWarning ( "TODO : Highlight.SwitchMethods._Switch (): Validate if entity exists" ) ;
         }

        /// <summary>
        /// Disable old highlight and enable new one.
        /// </summary>
        /// <param name="newEntityToHiglght"></param>
        static public void _Switch ( EntityCommandBuffer ecb, Entity newEntityToHiglght )
        {
      
            // Switch if previous and current entities are different
            if ( !previousHighlightedEntity.Equals ( newEntityToHiglght ) )
            {   
                // Restore previous material
                if ( previousHighlightedEntity.Version > 0 ) ecb.AddComponent ( previousHighlightedEntity, new ResetHighlightTag () ) ;
                                
                previousHighlightedEntity = newEntityToHiglght ;

                // Test highlight
                // EntityManager.AddComponent ( newEntityToHiglght, typeof ( Blocks.BlockSetHighlightTag ) ) ;
                ecb.AddComponent ( newEntityToHiglght, new SetHighlightTag () ) ;
            }

        }

    }
}
