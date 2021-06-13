using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody),typeof(MeshFilter),typeof(MeshCollider))]
public class AwesomeRigidbody : MonoBehaviour
{
    //메시
    private Mesh meshToRender;
    private Mesh meshForCollision;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private new Rigidbody rigidbody;



    [SerializeField] private GameObject debugObj;
    [SerializeField] private float margin;
    [SerializeField] private float limitForce;
    private List<GameObject> bCollObject;
    private void Awake()
    {
        //기본 컴포넌트
        meshFilter = GetComponent<MeshFilter>();
        rigidbody = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null)
            Debug.LogError("meshFilter is null");
        if (rigidbody == null)
            Debug.LogError("rigidbody is null");
        meshToRender = meshFilter.mesh;

        //
        bCollObject = new List<GameObject>();

        //충돌인식을 위한 새 매시 계산
        meshForCollision = new Mesh();
        List<Vector3> verticesToCollision = new List<Vector3>();
        for (int i = 0; i < meshToRender.vertices.Length; ++i)
        {
            verticesToCollision.Add(meshToRender.vertices[i] + meshToRender.vertices[i].normalized * margin);
        }
        meshForCollision.SetVertices(verticesToCollision);
        meshForCollision.triangles = meshToRender.triangles.Clone() as int[];
        meshForCollision.normals = meshToRender.normals.Clone() as Vector3[];
        meshFilter.mesh = meshForCollision;

        //
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshForCollision;
        meshCollider.convex = true;
        meshCollider.isTrigger = false;

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            rigidbody.AddForce(transform.up * 100.0f);
        }
        Vector3 v = rigidbody.velocity;
        rigidbody.Sleep();
        rigidbody.WakeUp();
        rigidbody.velocity = v;

        //Debug Draw
        //DebugDraw(meshToCollision,transform.localToWorldMatrix,transform.position, Color.red);
    }
    private void OnCollisionEnter(Collision collision)
    {
        AwesomeRigidbody otherAR = collision.gameObject.GetComponent<AwesomeRigidbody>();
        if (otherAR == null)
            return;

        //true || 부분은 디버깅용
        if (true || collision.contactCount == 1)
        {
            //Debug.Log(name + " 이 " + collision.gameObject.name + "와 부딪힘");
            otherAR.CrashEvent(collision);
        }
        //CalculateCollision(collision.collider);
    }
    //Unused
    void CalculateCollision(Collider coll)
    {
        //DebugDraw(coll.GetComponent<MeshFilter>().mesh, coll.transform.localToWorldMatrix, coll.transform.position, Color.blue);
        float collisionVelocityMag = coll.transform.GetComponent<Rigidbody>().velocity.magnitude + rigidbody.velocity.magnitude;
        //Debug.Log(collisionVelocityMag.ToString() + " > " + limitForce + " = " + (collisionVelocityMag > limitForce));
        if (collisionVelocityMag > limitForce)
        {
            //충돌 검사
            int hitCount = 0;
            HashSet<Vector3> hitVertices = new HashSet<Vector3>();
            for (int i = 0; i < meshForCollision.triangles.Length - 1; i++)
            {
                int s, e;
                if ((i + 1) % 3 == 0)
                {
                    s = meshToRender.triangles[i];
                    e = meshToRender.triangles[i - 2];
                }
                else
                {
                    s = meshForCollision.triangles[i];
                    e = meshForCollision.triangles[i + 1];
                }
                Matrix4x4 m = transform.localToWorldMatrix;
                Vector3 vec3_s = m.MultiplyVector(meshForCollision.vertices[s]) + transform.position;
                Vector3 vec3_e = m.MultiplyVector(meshForCollision.vertices[e]) + transform.position;

                if (CheckLineIsInCollision(coll, vec3_s, vec3_e))
                {
                    if (!hitVertices.Add(vec3_s))
                        hitCount++;
                    if (!hitVertices.Add(vec3_e))
                        hitCount++;
                    Debug.DrawLine(vec3_s, vec3_e, Color.green);
                    //hitCount++;
                }
            }
            //hitCount = hitVertices.Count;

            //충돌후처리 (충돌된 line이 4개이상이면)
            if (hitCount >= 8)
            {
                ;// Debug.Log("파괴");
            }
            //Debug.Log("hitCount = " + hitCount);
        }
    }
    //날 공격함.
    void CrashEvent(Collision collision)
    {
        float crashPower = collision.relativeVelocity.magnitude / limitForce;
        if (crashPower >= 1)
        {
            Debug.Log(gameObject.name + "이 받은 crashPower is " + crashPower);
            int[] triangles = meshForCollision.triangles;
            List<Vector3[]> realPositionTriangles = new List<Vector3[]>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //실제좌표
                Vector3 v1, v2, v3, pos = transform.position;
                Matrix4x4 m = transform.localToWorldMatrix;
                v1 = m.MultiplyVector(meshForCollision.vertices[triangles[i]]) + pos;
                v2 = m.MultiplyVector(meshForCollision.vertices[triangles[i + 1]]) + pos;
                v3 = m.MultiplyVector(meshForCollision.vertices[triangles[i + 2]]) + pos;

                realPositionTriangles.Add(new Vector3[] { v1, v2, v3 });
            }

            ContactPoint cp = collision.GetContact(0);  //충돌지점
            Vector3 forceDir = -cp.normal;      //충돌방향벡터
            Debug.DrawLine(cp.point, cp.point + forceDir * 10, Color.red);//Debug

            //충돌시 가장 윗판과 아랫판을 알아냄 (triangle)
            List<List<Vector3>> topAndBottom = new List<List<Vector3>>();
            //모든 triangle에 대해 충돌벡터와 충돌체크
            foreach (Vector3[] vs in realPositionTriangles)
            {
                Vector3 collPoint;
                float t;
                if (RayTriangle_Intersect(cp.point, forceDir, vs[0], vs[1], vs[2], out t))
                {
                    collPoint = cp.point + forceDir * t;
                    List<Vector3> vector3s = new List<Vector3> { vs[0], vs[1], vs[2], collPoint };
                    topAndBottom.Add(vector3s);
                }
            }


            int topIdx = -1;
            int bottomIdx = -1;
            float minF = 10000.0f, maxF = 0;
            foreach (List<Vector3> vls in topAndBottom)
            {
                float curF;
                RayTriangle_Intersect(cp.point, forceDir, vls[0], vls[1], vls[2], out curF);
                if (maxF < curF)
                {
                    maxF = curF;
                    bottomIdx = topAndBottom.IndexOf(vls);
                }
                if (minF > curF)
                {
                    minF = curF;
                    topIdx = topAndBottom.IndexOf(vls);
                }
            }
            //윗판과 아랫판에 충돌한 위치
            Vector3 topCollPoint = topAndBottom[topIdx][3];
            topAndBottom[topIdx].RemoveAt(3);
            Vector3 bottomCollPoint = topAndBottom[bottomIdx][3];
            topAndBottom[bottomIdx].RemoveAt(3);

            //인접한 mesh찾기
            List<Vector3[]> topTries, bottomTries;
            //Top
            VertexLib.GetPlane(out bottomTries, topAndBottom[bottomIdx], realPositionTriangles);
            //Bottom
            VertexLib.GetPlane(out topTries, topAndBottom[topIdx], realPositionTriangles);



            //인접한 mesh DebugDraw
            foreach (Vector3[] vs in bottomTries)
            {
                Debug.DrawLine(vs[0], vs[1], Color.red);
                Debug.DrawLine(vs[1], vs[2], Color.red);
                Debug.DrawLine(vs[2], vs[0], Color.red);
            }
            foreach (Vector3[] vs in topTries)
            {
                Debug.DrawLine(vs[0], vs[1], Color.cyan);
                Debug.DrawLine(vs[1], vs[2], Color.cyan);
                Debug.DrawLine(vs[2], vs[0], Color.cyan);
            }

            //TopPlane에서 정렬된 정점수집
            List<Vector3> orderedTopVertices, orderedBottomVertices;
            VertexLib.GetOrderedVertices(out orderedTopVertices, topTries);
            VertexLib.GetOrderedVertices(out orderedBottomVertices, bottomTries);

            //DebugObject
            int count = 0;
            //foreach (Vector3 v in orderedTopVertices)
            //    Instantiate(debugObj, v, Quaternion.identity).name = "Top " + count++;
            //count = 0;
            //foreach (Vector3 v in orderedBottomVertices)
            //    Instantiate(debugObj, v, Quaternion.identity).name = "Bottom " + count++;

            //정점 선택
            Vector3 closestV0, closestV1;
            for (int i = 0; i < orderedTopVertices.Count; ++i)
            {
                int p1 = i;
                int p2 = i + 1;
                if (i == orderedTopVertices.Count - 1)
                    p2 = 0;
                VertexLib.GetClosestVertex(out closestV0, orderedTopVertices[i], orderedBottomVertices);
                VertexLib.GetClosestVertex(out closestV1, orderedTopVertices[(i + 1) % orderedTopVertices.Count], orderedBottomVertices);
                Vector3[] v6 = new Vector3[18];
                v6[0] = orderedTopVertices[p1];
                v6[1] = orderedTopVertices[p2];
                v6[2] = topCollPoint;

                v6[3] = closestV0;
                v6[4] = closestV1;
                v6[5] = bottomCollPoint;

                v6[6] = v6[1];
                v6[7] = v6[4];
                v6[8] = v6[5];
                v6[9] = v6[2];

                v6[10] = v6[2];
                v6[11] = v6[5];
                v6[12] = v6[3];
                v6[13] = v6[0];

                v6[14] = v6[0];
                v6[15] = v6[3];
                v6[16] = v6[4];
                v6[17] = v6[1];
                CreateSagment(v6, topCollPoint);
            }

            //Create Gameobject
            Destroy(gameObject);

        }
    }
    void DebugDraw(Mesh meshToDebug, Matrix4x4 localToWorldMatrix, Vector3 pos, Color color)
    {
        for (int i = 0; i < meshToDebug.triangles.Length; i += 3)
        {
            int t1 = meshToDebug.triangles[i];
            int t2 = meshToDebug.triangles[i + 1];
            int t3 = meshToDebug.triangles[i + 2];

            Debug.DrawLine(localToWorldMatrix.MultiplyVector(meshToDebug.vertices[t1]) + pos, localToWorldMatrix.MultiplyVector(meshToDebug.vertices[t2]) + pos, color);
            Debug.DrawLine(localToWorldMatrix.MultiplyVector(meshToDebug.vertices[t2]) + pos, localToWorldMatrix.MultiplyVector(meshToDebug.vertices[t3]) + pos, color);
            Debug.DrawLine(localToWorldMatrix.MultiplyVector(meshToDebug.vertices[t3]) + pos, localToWorldMatrix.MultiplyVector(meshToDebug.vertices[t1]) + pos, color);
        }
    }
    bool CheckLineIsInCollision(Collider coll, Vector3 begin, Vector3 end)
    {
        if (coll.bounds.Contains(begin) || coll.bounds.Contains(end))
            return true;
        return false;
    }
    // o is ray's original, d is direction, v1, v2, v3 is triangle coordinate t는 얼마나 연장하면 닿는가
    bool RayTriangle_Intersect(Vector3 o, Vector3 d, Vector3 v1, Vector3 v2, Vector3 v3, out float t)
    {
        float epsilon = 1.0e-5f;
        t = float.MaxValue;
        Vector3 e1 = v2 - v1;
        Vector3 e2 = v3 - v1;
        Vector3 p = Vector3.Cross(d, e2);
        float a = Vector3.Dot(e1, p);
        if (a > -epsilon && a < epsilon)
            return false;

        float f = 1 / a;
        Vector3 s = o - v1;
        float u = f * Vector3.Dot(s, p);
        if (u < 0 || u > 1)
            return false;

        Vector3 q = Vector3.Cross(s, e1);
        float v = f * Vector3.Dot(d, q);
        if (v < 0 || u + v > 1)
            return false;

        t = f * Vector3.Dot(e2, q);
        return true; // u, v, t is solved
    }
    void CreateSagment(Vector3[] verties18, Vector3 CollPoint)
    {
        Mesh newMesh = new Mesh();
        List<Vector3> newVertices = new List<Vector3>();
        //조각난 윗판 
        newVertices.AddRange(verties18);
        //정점완료
        newMesh.SetVertices(newVertices);


        //tries잡기
        int[] newTriangles = new int[24];
        newTriangles[0] = 0;
        newTriangles[1] = 1;
        newTriangles[2] = 2;

        newTriangles[3] = 3;
        newTriangles[4] = 5;
        newTriangles[5] = 4;

        newTriangles[6] = 6;
        newTriangles[7] = 7;
        newTriangles[8] = 8;

        newTriangles[9] = 6;
        newTriangles[10] = 8;
        newTriangles[11] = 9;

        newTriangles[12] = 10;
        newTriangles[13] = 11;
        newTriangles[14] = 12;

        newTriangles[15] = 10;
        newTriangles[16] = 12;
        newTriangles[17] = 13;

        newTriangles[18] = 14;
        newTriangles[19] = 15;
        newTriangles[20] = 16;

        newTriangles[21] = 14;
        newTriangles[22] = 16;
        newTriangles[23] = 17;
        for (int i = 0; i < newTriangles.Length; ++i)
        {
            Vector3 v = newVertices[newTriangles[i]];
            Instantiate(debugObj, v, Quaternion.identity).name = "newTriesVertex " + i;
        }
        newMesh.triangles = newTriangles;
        newMesh.RecalculateTangents();
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        GameObject go = new GameObject("jogak");
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mf.mesh = newMesh;
        mr.material = SimulationController.instance.GetRandomMat();
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        mr.GetPropertyBlock(properties);
        properties.SetColor("RandomColor", Random.ColorHSV());
        mr.SetPropertyBlock(properties);
        Rigidbody rigid = go.AddComponent<Rigidbody>();
        AwesomeRigidbody ar = go.AddComponent<AwesomeRigidbody>();
        ar.limitForce = limitForce;
        ar.debugObj = debugObj;
        StartCoroutine(sendForce(rigid, CollPoint));
    }
    IEnumerator sendForce(Rigidbody r, Vector3 pos)
    {
        yield return new WaitForSeconds(1.0f);
        r.AddExplosionForce(10000.0f, pos, 100.0f, 1000.0f, ForceMode.Impulse);
    }
}