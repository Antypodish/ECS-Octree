using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOctree_origin : MonoBehaviour
{
    BoundsOctree <int> boundsOctree ;

    // Start is called before the first frame update
    void Start()
    {
        boundsOctree = new BoundsOctree <int> ( 8, Vector3.zero, 1, 1 ) ;
        // boundsOctree = new BoundsOctree <int> ( 10, Vector3.zero, 4, 1 ) ;

        //boundsOctree.Add ( 10, new Bounds () { center = Vector3.one * 5, size = Vector3.one * 5 } ) ;
        //boundsOctree.Add ( 11, new Bounds () { center = Vector3.one * 15, size = Vector3.one * 5 } ) ;
        //_OctreeAddInstance ( 12, new Bounds () { center = Vector3.one * 25, size = Vector3.one * 5 } ) ;
        //boundsOctree.Add ( 12, new Bounds () { center = Vector3.one * 25, size = Vector3.one * 5 } ) ;

        //boundsOctree.Add ( 4, new Bounds () { center = new Vector3 ( 0, 0, 0 ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;
        //boundsOctree.Add ( 5, new Bounds () { center = new Vector3 ( 0, 0, 5 ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;
        //boundsOctree.Add ( 6, new Bounds () { center = new Vector3 ( 1, 0, 6 ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;


        
        for ( int i = 0; i < 100; i ++ )
        {            
            int x = i % 10 ;
            Debug.LogWarning ( "x: " + i + "; " + ((float) i % 10f ) ) ;
            int y = Mathf.FloorToInt ( i / 10 ) ;
            Debug.Log ( "Test instance spawn #" + i + " x: " + x + " y: " + y ) ;
            boundsOctree.Add ( i, new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;
            // _OctreeAddInstance ( i, new Bounds () { center = new Vector3 ( x, 0, y ) + Vector3.one * 0.5f, size = Vector3.one * 1 } ) ;
        }
        
        

//        boundsOctree.Remove ( 11 ) ;

        int i_instanceIndex ;

        Bounds checkBounds = new Bounds () { center = Vector3.one * 30, size = Vector3.one * 4 } ;
        bool isColliding = boundsOctree.IsColliding ( checkBounds, out i_instanceIndex ) ;
        Debug.Log ( "Orig Colliding: " + ( isColliding ? "T" : "F") ) ;

        checkBounds = new Bounds () { center = Vector3.one * 0, size = Vector3.one * 30 } ;
        isColliding = boundsOctree.IsColliding ( checkBounds, out i_instanceIndex ) ;
        Debug.Log ( "Orig Colliding: " + ( isColliding ? "T" : "F") ) ;

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray () ;
        ray = Camera.main.ScreenPointToRay ( Input.mousePosition ) ; // .ViewportToWorldPoint ( Input.mousePosition ) ;
        Debug.DrawLine ( ray.origin, ray.origin + ray.direction * 1000, Color.red ) ;
        
        int i_instanceIndex ;
        bool isColliding = boundsOctree.IsColliding ( ray, out i_instanceIndex, 1000 ) ;

        if ( isColliding )
        {
            Debug.Log ( "Colliding #" + i_instanceIndex + "; " + Time.time ) ;
        }
    }

    private void OnDrawGizmos ( )
    {   
        if ( boundsOctree != null ) boundsOctree.DrawAllBounds ( ) ;
    }
}
