using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sebastian.Geometry;

public class ShapeCreator : MonoBehaviour {

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    [HideInInspector]
    public List<Shape> shapes = new List<Shape>();

    [HideInInspector]
    public bool showShapesList;

    public float handleRadius = 0.2f;

    public void UpdateCollider()
    {
        //meshCollider.sharedMesh = compShape.GetMesh();
    }

    public void UpdateMeshDisplay()
    {
        CompositeShape compShape = new CompositeShape(shapes);
        meshFilter.mesh = compShape.GetMesh();
        //if (meshFilter.sharedMesh)
        //meshCollider.sharedMesh = meshFilter.sharedMesh;

        meshCollider.sharedMesh = compShape.GetMesh();
    }
}