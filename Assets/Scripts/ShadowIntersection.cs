using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShadowIntersection : MonoBehaviour {

    [SerializeField]
    private ShapeCreator shapeCreator;
    [SerializeField]
    private ShadowCaster shadowCaster;

    [SerializeField]
    private int stageShape = 1;

    private List<Vector2> GetInnerStageVertex()
    {
        int numPoints = shapeCreator.shapes[stageShape].points.Count;
        List<Vector2> intersectionsList = new List<Vector2>();
        SegmentIntersection intersection = gameObject.GetComponent<SegmentIntersection>();
        Vector3[] stageMeshVertices = new Vector3[numPoints];
        Vector2 intersectionVector = new Vector2();
        for (int i = 0; i < numPoints; i++)
        {
            stageMeshVertices[i] = shapeCreator.shapes[stageShape].points[i];
        }

        //foreach shadowMeshes pair of vertices we check each stageMeshVertices
        for (int i = 0; i < shadowCaster.shadowMeshes.Count; i++)
        {

            for (int j = 0; j < shadowCaster.shadowMeshes[i].vertices.Length; j++)
            {

                int j2 = 0;

                if (!(j == shadowCaster.shadowMeshes[i].vertices.Length - 1))
                {
                    j2 = j + 1;
                }
                Vector2 shadowVert1 = new Vector2(shadowCaster.shadowMeshes[i].vertices[j].x, shadowCaster.shadowMeshes[i].vertices[j].z);
                Vector2 shadowVert2 = new Vector2(shadowCaster.shadowMeshes[i].vertices[j2].x, shadowCaster.shadowMeshes[i].vertices[j2].z);
                for (int k = 0; k < stageMeshVertices.Length; k++)
                {
                    int k2 = 0;

                    if (!(k == stageMeshVertices.Length - 1))
                    {
                        k2 = k + 1;
                    }

                    Vector2 stageVert1 = new Vector2(stageMeshVertices[k].x, stageMeshVertices[k].z);
                    Vector2 stageVert2 = new Vector2(stageMeshVertices[k2].x, stageMeshVertices[k2].z);



                    if (intersection.LineIntersection(stageVert1, stageVert2, shadowVert1, shadowVert2, ref intersectionVector))
                    {
                        intersectionsList.Add(intersectionVector);
                        //Debug.Log(intersectionVector);

                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Destroy(sphere, 0.01f);
                        sphere.transform.position = new Vector3(intersectionVector.x, 0, intersectionVector.y);
                        sphere.transform.localScale = Vector3.one * 0.1f;
                    }

                }
            }
        }
        return intersectionsList;
    }

    private void LateUpdate()
    {
        List<Vector2> testIntersection = GetInnerStageVertex();
    }
}
