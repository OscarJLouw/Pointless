using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public struct RangePointPair
{
    public float intersectionRange;
    public Vector2 worldPosition;

    public RangePointPair(float _intersectionRange, Vector2 _worldPosition)
    {
        intersectionRange = _intersectionRange;
        worldPosition = _worldPosition;
    }
}

public class ShadowRange
{
    public int wallSegmentIndex;
    public int meshIndex;
    public List<RangePointPair> rangePointPairs;
    //public Vector2 point;

    public ShadowRange()
    {
        rangePointPairs = new List<RangePointPair>();
    }

}

public class ShadowVertex
{
    public int shapeIndex;
    public Vector3 position;

    public ShadowVertex(int _shapeIndex, Vector3 _position)
    {
        shapeIndex = _shapeIndex;
        position = _position;
    }
    public ShadowVertex(int _shapeIndex, Vector2 _2DPosition)
    {
        shapeIndex = _shapeIndex;
        position = new Vector3(_2DPosition.x, 0, _2DPosition.y);
    }
}

public class ShadowIntersection : MonoBehaviour {

    [SerializeField]
    private Transform player;

    [SerializeField]
    private ShapeCreator shapeCreator;
    [SerializeField]
    private ShadowCaster shadowCaster;

    [SerializeField]
    private int stageShape = 1;

    private List<ShadowRange> shadowRanges = new List<ShadowRange>();

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
            for (int k = 0; k < stageMeshVertices.Length; k++)
            {
                int k2 = 0;

                if (!(k == stageMeshVertices.Length - 1))
                {
                    k2 = k + 1;
                }

                Vector2 stageVert1 = new Vector2(stageMeshVertices[k].x, stageMeshVertices[k].z);
                Vector2 stageVert2 = new Vector2(stageMeshVertices[k2].x, stageMeshVertices[k2].z);

                ShadowRange shadowRange = new ShadowRange();
                shadowRange.meshIndex = i;
                shadowRange.wallSegmentIndex = k;

                for (int j = 0; j < shadowCaster.shadowMeshes[i].vertices.Length; j++)
                {

                    int j2 = 0;

                    if (!(j == shadowCaster.shadowMeshes[i].vertices.Length - 1))
                    {
                        j2 = j + 1;
                    }
                    Vector2 shadowVert1 = new Vector2(shadowCaster.shadowMeshes[i].vertices[j].x, shadowCaster.shadowMeshes[i].vertices[j].z);
                    Vector2 shadowVert2 = new Vector2(shadowCaster.shadowMeshes[i].vertices[j2].x, shadowCaster.shadowMeshes[i].vertices[j2].z);

                    if (intersection.LineIntersection(stageVert1, stageVert2, shadowVert1, shadowVert2, ref intersectionVector))
                    {
                        intersectionsList.Add(intersectionVector);

                        float distanceAlongStageEdge = (stageVert2 - stageVert1).magnitude / (stageVert1 - intersectionVector).magnitude;

                        shadowRange.rangePointPairs.Add(new RangePointPair(distanceAlongStageEdge, intersectionVector));

                        //Debug.Log(intersectionVector);
                        /*
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Destroy(sphere, 0.01f);
                        sphere.transform.position = new Vector3(intersectionVector.x, 0, intersectionVector.y);
                        sphere.transform.localScale = Vector3.one * (intersectionEdgeCount*0.01f + 0.05f);
                        */
                    }
                }

                shadowRanges.Add(shadowRange);
            }
        }
        return intersectionsList;
    }
    
    private void ComputeShadowBoundaries()
    {

        List<ShadowVertex> allShadowEdges = new List<ShadowVertex>();

        for (int borderVertexIndex = 0; borderVertexIndex < shapeCreator.shapes[stageShape].points.Count; borderVertexIndex++)
        {
            Vector3 vertex = shapeCreator.shapes[stageShape].points[borderVertexIndex];

            //allShadowEdges.Add(new ShadowVertex(stageShape, vertex));
            Debug.DrawLine(player.position, vertex);
            RaycastHit hit = new RaycastHit();
            if(Physics.Raycast(player.position, (player.position - vertex).normalized, out hit))
            {
                Debug.Log("Hi");
                allShadowEdges.Add(new ShadowVertex(stageShape, vertex));
            }
        }

        for (int i = 0; i < shadowRanges.Count; i++)
        {
            if (shadowRanges[i].rangePointPairs.Count > 0)
            {
                List<RangePointPair> sortedRanges = shadowRanges[i].rangePointPairs.OrderBy(r => r.intersectionRange).ToList();

                Vector2 shadowStart = sortedRanges[0].worldPosition;
                Vector2 shadowEnd = sortedRanges[sortedRanges.Count-1].worldPosition;

                allShadowEdges.Add(new ShadowVertex(i, shadowStart));
                allShadowEdges.Add(new ShadowVertex(i, shadowEnd));

                /*
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphere, 0.01f);
                sphere.transform.position = new Vector3(shadowStart.x, 0, shadowStart.y);
                sphere.transform.localScale = Vector3.one * 0.2f;
                

                GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(sphere2, 0.01f);
                sphere2.transform.position = new Vector3(shadowEnd.x, 0, shadowEnd.y);
                sphere2.transform.localScale = Vector3.one * 0.1f;
                */
            }
        }

        for (int shadowVertex = 0; shadowVertex < allShadowEdges.Count; shadowVertex++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere, 0.01f);
            sphere.transform.position = allShadowEdges[shadowVertex].position;
            sphere.transform.localScale = Vector3.one * 0.1f;
        }
    }


    private void LateUpdate()
    {
        List<Vector2> testIntersection = GetInnerStageVertex();
        ComputeShadowBoundaries();

        shadowRanges.Clear();
    }
}
