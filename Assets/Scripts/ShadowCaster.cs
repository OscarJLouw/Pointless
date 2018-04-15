using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowCaster : MonoBehaviour
{
    [SerializeField]
    private ShapeCreator shapeCreator;

    public float maxRange = 30;

    //private List<MeshFilter> shadowMeshFilters = new List<MeshFilter>();


    [HideInInspector]
    public List<Mesh> shadowMeshes = new List<Mesh>();
    //private List<GameObject> shadowObjects = new List<GameObject>();

    [SerializeField]
    private Material shadowMaterial;
    [SerializeField]
    private int ignoreShapes = 2;

    private int lastNumberOfShapes = 0;

    void Start()
    {
        SetupMeshes();
        /*
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        */

        lastNumberOfShapes = shapeCreator.shapes.Count;
    }

    private void SetupMeshes()
    {
        for (int i = 0; i < shapeCreator.shapes.Count - ignoreShapes; i++)
        {
            Mesh shadowMesh = new Mesh();
            shadowMesh.name = "Shadow Mesh for Shape " + (i + ignoreShapes + 1);
            shadowMeshes.Add(shadowMesh);

            GameObject newShadowGameObject = new GameObject("Shadow for Shape " + (i + ignoreShapes + 1));
            newShadowGameObject.AddComponent<MeshFilter>().mesh = shadowMesh;
            newShadowGameObject.AddComponent<MeshRenderer>().material = shadowMaterial;
            newShadowGameObject.transform.position = Vector3.up * -0.1f;
            //shadowObjects.Add(newShadowGameObject);
        }

    }

    private void GenerateShadowMeshes()
    {

        if(lastNumberOfShapes != shapeCreator.shapes.Count)
        {
            SetupMeshes();
            
        }

        lastNumberOfShapes = shapeCreator.shapes.Count;

        for (int i = ignoreShapes; i < shapeCreator.shapes.Count; i++)
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
                (numPoints * 2 - 2) + "," + 
                (numPoints * 2 - 1) + "," + 
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
                (numPoints * 2 - 1) + "," +
                (0) + "," +
                (1));
            */

            shadowMeshes[i - ignoreShapes].Clear();
            shadowMeshes[i - ignoreShapes].vertices = shadowMeshVertices;
            shadowMeshes[i - ignoreShapes].triangles = shadowMeshIndices;
            shadowMeshes[i - ignoreShapes].RecalculateNormals();
        }
    }

    private void Update()
    {
        GenerateShadowMeshes();
    }
}
