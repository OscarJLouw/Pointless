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
    public int wallIndex;
    public Vector3 position;
    public float percentAlongWall;

    public ShadowVertex(int _shapeIndex, int _wallIndex, Vector3 _position, float _percentAlongWall)
    {
        shapeIndex = _shapeIndex;
        wallIndex = _wallIndex;
        position = _position;
        percentAlongWall = _percentAlongWall;
    }
    public ShadowVertex(int _shapeIndex, int _wallIndex, Vector2 _2DPosition, float _percentAlongWall)
    {
        shapeIndex = _shapeIndex;
        wallIndex = _wallIndex;
        position = new Vector3(_2DPosition.x, 0, _2DPosition.y);
        percentAlongWall = _percentAlongWall;
    }
}

public class ShadowPoint
{
    public float range;
    public Vector2 point;
    public int count; // +1 when enter shadow, -1 when exit shadow

    public ShadowPoint(float range, Vector2 point, int count)
    {
        this.range = range;
        this.point = point;
        this.count = count;
    }
}

public class BoundaryShadow
{
    public int boundaryIndex;
    public float minRange;
    public float maxRange;

    public BoundaryShadow(int boundaryIndex, float minRange, float maxRange)
    {
        this.boundaryIndex = boundaryIndex;
        this.minRange = minRange;
        this.maxRange = maxRange;
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
    List<ShadowVertex> allShadowVertices = new List<ShadowVertex>();

    SegmentIntersection intersection;

    private List<Vector2> GetInnerStageVertex()
    {
        int numPoints = shapeCreator.shapes[stageShape].points.Count;
        List<Vector2> intersectionsList = new List<Vector2>();
        intersection = gameObject.GetComponent<SegmentIntersection>();
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

                Vector2 vec2Player = new Vector2(player.position.x, player.position.z);
                //Vector2 rayFromPlayer = (stageVert1 - vec2Player)*0.99f;

                for(int shape = stageShape+1; shape < shapeCreator.shapes.Count; shape++)
                {
                    for (int p = 0; p < shapeCreator.shapes[shape].points.Count; p++)
                    {
                        Vector3 point1 = shapeCreator.shapes[shape].points[p];
                        Vector3 point2 = new Vector3();
                        if (!(p == shapeCreator.shapes[shape].points.Count - 1))
                            point2 = shapeCreator.shapes[shape].points[p+1];
                        else
                            point2 = shapeCreator.shapes[shape].points[0];

                        Vector2 pointVert1 = new Vector2(point1.x, point1.z);
                        Vector2 pointVert2 = new Vector2(point2.x, point2.z);


                        Vector2 tempIntersectionVector = new Vector2();
                        if (intersection.LineIntersection(vec2Player, stageVert1, pointVert1, pointVert2, ref tempIntersectionVector))
                        {

                            p = shapeCreator.shapes[shape].points.Count;
                            allShadowVertices.Add(new ShadowVertex(stageShape, k, stageVert1, 0));

                        }
                    }
                    
                }

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

                        float distanceAlongStageEdge = (intersectionVector - stageVert1).magnitude / (stageVert2 - stageVert1).magnitude;

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

        for (int i = 0; i < shadowRanges.Count; i++)
        {
            if (shadowRanges[i].rangePointPairs.Count > 0)
            {
                List<RangePointPair> sortedRanges = shadowRanges[i].rangePointPairs.OrderBy(sr => sr.intersectionRange).ToList();

                Vector2 shadowStart = sortedRanges[0].worldPosition;
                Vector2 shadowEnd = sortedRanges[sortedRanges.Count-1].worldPosition;

                allShadowVertices.Add(new ShadowVertex(i, shadowRanges[i].wallSegmentIndex, shadowStart, sortedRanges[0].intersectionRange));
                allShadowVertices.Add(new ShadowVertex(i, shadowRanges[i].wallSegmentIndex, shadowEnd, sortedRanges[sortedRanges.Count - 1].intersectionRange));

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

        // sort allShadowVertices by wall index
        // 
        
        List<ShadowVertex> sortedShadowVertices = allShadowVertices.OrderBy(asv => asv.wallIndex).ToList();

        List<List<ShadowVertex>> wallVertexCollections = new List<List<ShadowVertex>>();

        int wallIndex = sortedShadowVertices[0].wallIndex;

        wallVertexCollections.Add(new List<ShadowVertex>());



        List <List <ShadowPoint>> shadowsPerBoundary = new  List < List <ShadowPoint>>();
        for (int boundaryEdgeFrom = 0; boundaryEdgeFrom < shapeCreator.shapes[stageShape].points.Count; boundaryEdgeFrom++)
        {
            shadowsPerBoundary.Add(new List<ShadowPoint>());
        }
        for (int shapeIndex = stageShape+1; shapeIndex < shapeCreator.shapes.Count; shapeIndex++)
        {
            // iterate edges
            

            var shape = shapeCreator.shapes[shapeIndex];
            for (int i = 0; i < shape.points.Count; i++) {
                Vector3 edgeFrom = shape.points[i];
                Vector3 edgeTo = shape.points[(i+1)%shape.points.Count];

                Vector3 edgeFromFar = edgeFrom + (edgeFrom - player.position).normalized * shadowCaster.maxRange;
                Vector3 edgeToFar = edgeTo + (edgeTo - player.position).normalized * shadowCaster.maxRange;

                for (int boundaryEdgeFrom = 0; boundaryEdgeFrom < shapeCreator.shapes[stageShape].points.Count; boundaryEdgeFrom++)
                {
                    List<ShadowPoint> shadows = shadowsPerBoundary[boundaryEdgeFrom];
                    int boundaryEdgeTo = 0;

                    if (!(boundaryEdgeFrom == shapeCreator.shapes[stageShape].points.Count - 1))
                    {
                        boundaryEdgeTo = boundaryEdgeFrom + 1;
                    }

                    Vector2 stageVert1 = new Vector2(shapeCreator.shapes[stageShape].points[boundaryEdgeFrom].x, shapeCreator.shapes[stageShape].points[boundaryEdgeFrom].z);
                    Vector2 stageVert2 = new Vector2(shapeCreator.shapes[stageShape].points[boundaryEdgeTo].x, shapeCreator.shapes[stageShape].points[boundaryEdgeTo].z);

                    Vector2 edgeFromIntersectionVector = new Vector2();
                    bool edgeFromIntersection = (intersection.LineIntersection(edgeFrom, edgeFromFar, stageVert1, stageVert2, ref edgeFromIntersectionVector));


                    Vector2 edgeToIntersectionVector = new Vector2();
                    bool edgeToIntersection = (intersection.LineIntersection(edgeTo, edgeToFar, stageVert1, stageVert2, ref edgeToIntersectionVector));
                    if (edgeFromIntersection && edgeToIntersection) { // todo handle corner shapes
                        // sort vertices
                        if (Vector3.Distance(edgeFromIntersectionVector, stageVert1) > Vector3.Distance(edgeToIntersectionVector, stageVert1)) { // fix
                            var tmp = edgeFromIntersectionVector; // swap
                            edgeFromIntersectionVector = edgeToIntersectionVector;
                            edgeToIntersectionVector = tmp;
                        }
                        shadows.Add(new ShadowPoint(Vector3.Distance(edgeFromIntersectionVector, stageVert1), edgeFromIntersectionVector, +1));
                        shadows.Add(new ShadowPoint(Vector3.Distance(edgeToIntersectionVector, stageVert1), edgeToIntersectionVector, -1));
                    }

                }

            }
        }

        List<BoundaryShadow> boundaryShadows = new List<BoundaryShadow>();

        for(int boundary = 0; boundary < shadowsPerBoundary.Count; boundary++)
        {
            int counter = 0;
            List<ShadowPoint> sortedShadowPointList = shadowsPerBoundary[boundary].OrderBy(o => o.range).ToList();

            foreach (ShadowPoint shadowPoint in sortedShadowPointList)
            {
                if (counter == 0)
                {
                    counter += shadowPoint.count;
                    if (counter > 0)
                        boundaryShadows.Add(new BoundaryShadow(boundary, shadowPoint.range, shadowPoint.range));
                    else
                        Debug.Log("Oh dear - closed a shadow that wasn't opened");
                } else
                {
                    counter += shadowPoint.count;
                    boundaryShadows[boundaryShadows.Count-1].maxRange = shadowPoint.range;
                }

            }
        }

        foreach (var shadow in boundaryShadows)
        {

            Vector3 boundaryStartPoint = shapeCreator.shapes[stageShape].points[shadow.boundaryIndex];
            Vector3 boundaryEndPoint = shapeCreator.shapes[stageShape].points[(shadow.boundaryIndex+1) % (shapeCreator.shapes[stageShape].points.Count)];
            
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere, 0.01f);
            sphere.transform.position = boundaryStartPoint + (boundaryEndPoint- boundaryStartPoint).normalized * shadow.minRange * (boundaryEndPoint - boundaryStartPoint).magnitude;
            sphere.transform.localScale = Vector3.one * 0.2f;
            
            
            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere2, 0.01f);
            sphere2.transform.position = boundaryStartPoint + (boundaryEndPoint - boundaryStartPoint).normalized * shadow.maxRange * (boundaryEndPoint - boundaryStartPoint).magnitude;
            sphere2.transform.localScale = Vector3.one * 0.2f;
            
        }



        /*
        for (int shadowVertex = 0; shadowVertex < sortedShadowVertices.Count; shadowVertex++)
        {
            if(sortedShadowVertices[shadowVertex].wallIndex == wallIndex)
            {
                wallVertexCollections[wallVertexCollections.Count-1].Add(sortedShadowVertices[shadowVertex]);

                
            } else
            {
                wallVertexCollections[wallVertexCollections.Count - 1].Add(sortedShadowVertices[shadowVertex]);
                wallIndex = sortedShadowVertices[shadowVertex].wallIndex;
                wallVertexCollections.Add(new List<ShadowVertex>());
            }
            
            //if(sortedShadowVertices[shadowVertex])
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere, 0.01f);
            sphere.transform.position = sortedShadowVertices[shadowVertex].position;
            sphere.transform.localScale = Vector3.one * (0.1f + 0.1f * sortedShadowVertices[shadowVertex].wallIndex);
            
        }
        */


        /*
        for (int vertexWallListIndex = 0; vertexWallListIndex < wallVertexCollections.Count; vertexWallListIndex++)
        {
            // sort all walls by their percentage along the parent wall
             List<ShadowVertex> thisWallVertexList = wallVertexCollections[vertexWallListIndex].OrderBy(wvc => wvc.percentAlongWall).ToList();
            
            Debug.Log("vertexWallListIndex: " + vertexWallListIndex);
            Debug.Log("Min: " + thisWallVertexList[0].percentAlongWall);
            Debug.Log("Max: " + thisWallVertexList[thisWallVertexList.Count - 1].percentAlongWall);

            int k = thisWallVertexList[0].wallIndex;
            int k2 = 0;
            
            if (!(k == shapeCreator.shapes[stageShape].points.Count - 1))
            {
                k2 = k + 1;
            }

            int l = thisWallVertexList[thisWallVertexList.Count - 1].wallIndex;
            int l2 = 0;

            if (!(l == shapeCreator.shapes[stageShape].points.Count - 1))
            {
                l2 = l + 1;
            }

            Vector2 stageVert1FP1 = new Vector2(shapeCreator.shapes[stageShape].points[k].x, shapeCreator.shapes[stageShape].points[k].z);
            Vector2 stageVert2FP1 = new Vector2(shapeCreator.shapes[stageShape].points[k2].x, shapeCreator.shapes[stageShape].points[k2].z);

            Vector2 stageVert1FP2 = new Vector2(shapeCreator.shapes[stageShape].points[l].x, shapeCreator.shapes[stageShape].points[l].z);
            Vector2 stageVert2FP2 = new Vector2(shapeCreator.shapes[stageShape].points[l2].x, shapeCreator.shapes[stageShape].points[l2].z);


            Vector2 normalisedDirectionFP1 = (stageVert2FP1 - stageVert1FP1).normalized;
            Vector2 normalisedDirectionFP2 = (stageVert2FP2 - stageVert1FP2).normalized;

            Vector2 finalPosition1 = stageVert1FP1 + normalisedDirectionFP1 * (stageVert2FP1 - stageVert1FP1).magnitude * thisWallVertexList[0].percentAlongWall;
            Vector2 finalPosition2 = stageVert1FP2 + normalisedDirectionFP2 * (stageVert2FP2 - stageVert1FP2).magnitude * thisWallVertexList[thisWallVertexList.Count - 1].percentAlongWall;

            Vector3 objectTransform1 = new Vector3(finalPosition1.x, 0, finalPosition1.y);
            Vector3 objectTransform2 = new Vector3(finalPosition2.x, 0, finalPosition2.y);

            
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere, 0.01f);
            sphere.transform.position = objectTransform1;
            sphere.transform.localScale = Vector3.one * 0.1f;
            

            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere2, 0.01f);
            sphere2.transform.position = objectTransform2;
            sphere2.transform.localScale = Vector3.one * 0.1f;
            
            
            GameObject sphere3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere3, 0.01f);
            sphere3.transform.position = thisWallVertexList[0].position;
            sphere3.transform.localScale = Vector3.one * 0.1f;

            GameObject sphere4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(sphere4, 0.01f);
            sphere4.transform.position = thisWallVertexList[thisWallVertexList.Count-1].position;
            sphere4.transform.localScale = Vector3.one * 0.1f;
            
            /*
            /*
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;

            for (int shadowVertices = 0; shadowVertices < wallVertexCollections[vertexWallListIndex].Count; shadowVertices++)
            {
                if
                wallVertexCollections[vertexWallListIndex]
            }
            
        }
        */
        allShadowVertices.Clear();
    }


    private void LateUpdate()
    {
        List<Vector2> testIntersection = GetInnerStageVertex();
        ComputeShadowBoundaries();

        shadowRanges.Clear();
    }
}
