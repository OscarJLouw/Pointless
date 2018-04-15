using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Lock. Solution to the puzzle
public class OutlineLock : MonoBehaviour {

    private LineRenderer lineRenderer;
    private Vector2[] intersections;
    private Vector2[] testIntersections = new Vector2[3];
    public float distanceToOutline = 1.06f;
    // Use this for initialization
    void Start()
    {
        //test points
        testIntersections[0] = new Vector2((float)-0.5097109, (float)3.74515);
        testIntersections[1] = new Vector2((float)-0.006582094, (float)3.996876);
        testIntersections[2] = new Vector2((float)0.5090174, (float)3.750584);

        lineRenderer = GetComponent<LineRenderer>();
        SetLockOutline(testIntersections);
    }

    public void SetLockOutline(Vector2[] key)
    {
        intersections = new Vector2[key.Length];
        for (int i = 0; i < intersections.Length; i++)
        {
            intersections[i] = key[i];
        }

        //setting spheres at dots positions. for debugging.
        /* for(int i=0; i < testIntersections.Length; i++)
         {
             GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
             Destroy(sphere, 0.01f);
             sphere.transform.position = new Vector3(testIntersections[i].x, 0, testIntersections[i].y);
             sphere.transform.localScale = Vector3.one * 0.1f;
         }*/
       
         ProjectOutline(intersections);
        
    }

    public void ProjectOutline(Vector2[] points)
    {
        lineRenderer.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(points[i].x * distanceToOutline, 0, points[i].y * distanceToOutline));
        }

    }

}
