using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EdgeRange
{
    public float dist;
    public int count;
    public int stageEdgeInt;

    public EdgeRange(float _dist, int _count, int _stageEdgeInt)
    {
        dist = _dist;
        count = _count;
        stageEdgeInt = _stageEdgeInt;
    }
}

public class ShadowSegment
{
    public float startDist;
    public float endDist;

    public ShadowSegment(float _startDist, float _endDist)
    {
        startDist = _startDist;
        endDist = _endDist;
    }
}

public class Shadow
{
    public float startDist;
    public float endDist;

    public Shadow(float _startDist, float _endDist)
    {
        startDist = _startDist;
        endDist = _endDist;
    }
}


public class ShadowIntersection : MonoBehaviour {

    [SerializeField]
    private ShapeCreator shapeCreator;
    [SerializeField]
    private ShadowCaster shadowCaster;

    [SerializeField]
    private int stageShape = 1;

    private int intersectionEdgeCount = 0;
    private bool openedEdge = false;

    private List<EdgeRange> edgeRanges = new List<EdgeRange>();

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

                        float distanceAlongStageEdge = (stageVert2 - stageVert1).magnitude / (stageVert1 - intersectionVector).magnitude;
                        edgeRanges.Add(new EdgeRange(distanceAlongStageEdge, intersectionEdgeCount, k));
                        if (openedEdge)
                        {
                            openedEdge = false;
                            intersectionEdgeCount++;
                        } else
                        {
                            openedEdge = true;
                        }
                        //Debug.Log(intersectionVector);
                        /*
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        Destroy(sphere, 0.01f);
                        sphere.transform.position = new Vector3(intersectionVector.x, 0, intersectionVector.y);
                        sphere.transform.localScale = Vector3.one * (intersectionEdgeCount*0.01f + 0.05f);
                        */
                    }

                    intersectionEdgeCount++;

                }
            }
        }
        return intersectionsList;
    }

    private void FindShadowBoundaries()
    {
        if (edgeRanges.Count > 0) {

            List<List<Shadow>> wallShadows = new List<List<Shadow>>();

            List<EdgeRange> SortedList = edgeRanges.OrderBy(e => e.stageEdgeInt).ToList();
            //List<Shadow> shadowList = new List<Shadow>();

            //List<Vector2> shadowEdges = new List<Vector2>();

            int stageEdgeIndex = SortedList[0].stageEdgeInt;
            wallShadows.Add(new List<Shadow>());

            for (int i = 0; i < edgeRanges.Count; i++)
            {
                EdgeRange thisEdgeRange = SortedList[i];
                if (thisEdgeRange.stageEdgeInt != stageEdgeIndex)
                {
                    wallShadows.Add(new List<Shadow>());
                    // reached next wall
                    stageEdgeIndex = thisEdgeRange.stageEdgeInt;
                } else
                {
                    List<Shadow> thisShadowList = wallShadows[wallShadows.Count];
                    for (int shadow = 0; shadow < thisShadowList.Count; shadow++)
                    {
                        //thisShadowList[shadow]
                    }
                    thisShadowList.Add(new Shadow(thisEdgeRange.dist, thisEdgeRange.dist));
                }
        }

        }
        /*
        if (!(k == stageMeshVertices.Length - 1))
        {
            k2 = k + 1;
        }

        Vector2 stageVert1 = new Vector2(stageMeshVertices[k].x, stageMeshVertices[k].z);
        Vector2 stageVert2 = new Vector2(stageMeshVertices[k2].x, stageMeshVertices[k2].z);
        */
    }

    private void LateUpdate()
    {
        List<Vector2> testIntersection = GetInnerStageVertex();
        intersectionEdgeCount = 0;
    }
}
