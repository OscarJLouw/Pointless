using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCaster : MonoBehaviour
{

    public ShapeCreator shapeCreator;

    public float maxRange = 30;

    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    private void GenerateShadowMeshes()
    {

        for (int i = 2; i < shapeCreator.shapes.Count; i++)
        {

            int numPoints = shapeCreator.shapes[i].points.Count;

            Vector3[] shadowMeshVertices = new Vector3[numPoints * 2];
            //int[] shadowMeshIndices = new int[((numPoints*2) - 2) * 3];
            int[] shadowMeshIndices = new int[numPoints * 6];
            for (int j = 0; j < numPoints; j++)
            {
                shadowMeshVertices[j * 2] = shapeCreator.shapes[i].points[j];

                Vector3 difference = (shapeCreator.shapes[i].points[j] - transform.position).normalized;
                shadowMeshVertices[j * 2 + 1] = difference * maxRange;

            }

            for (int j = 0; j < numPoints*2 - 2; j++)
            {
                shadowMeshIndices[j * 3] = j;
                shadowMeshIndices[j * 3 + 1] = j + 1;
                shadowMeshIndices[j * 3 + 2] = j + 2;

                //Debug.Log((j*3) + "," + (j * 3 + 1) + "," + (j * 3 + 2) + " || " + j + "," + (j+1) + "," + (j + 2));
            }

            shadowMeshIndices[(numPoints * 2 - 2) * 3 + 0] = numPoints * 2 - 2;
            shadowMeshIndices[(numPoints * 2 - 2) * 3 + 1] = numPoints * 2 - 1;
            shadowMeshIndices[(numPoints * 2 - 2) * 3 + 2] = 0;

            /*
            Debug.Log(
                ((numPoints * 2 - 2) * 3 + 0) + "," +
                ((numPoints * 2 - 2) * 3 + 1) + "," +
                ((numPoints * 2 - 2) * 3 + 2) + " || " + 
                (numPoints * 2 - 2 - 1) + "," + 
                (numPoints * 2 - 2) + "," + 
                (0));
            */

            shadowMeshIndices[(numPoints * 2 - 2) * 3 + 3] = numPoints * 2 - 1;
            shadowMeshIndices[(numPoints * 2 - 2) * 3 + 4] = 0;
            shadowMeshIndices[(numPoints * 2 - 2) * 3 + 5] = 1;

            /*
            Debug.Log(
                ((numPoints * 2 - 2) * 3 + 3) + "," +
                ((numPoints * 2 - 2) * 3 + 4) + "," +
                ((numPoints * 2 - 2) * 3 + 5) + " || " +
                (numPoints * 2 - 2) + "," +
                (0) + "," +
                (1));
            */

            viewMesh.Clear();
            viewMesh.vertices = shadowMeshVertices;
            viewMesh.triangles = shadowMeshIndices;
            viewMesh.RecalculateNormals();
        }
    }

    private void Update()
    {
        GenerateShadowMeshes();
    }
}
