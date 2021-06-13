using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class VertexLib : MonoBehaviour
{
    //메시를 구성하는 삼각형들로부터 정렬된 가장자리 정점리스트 얻기
    public static void GetOrderedVertices(
        out List<Vector3> orderedVertices,
        List<Vector3[]> srcTries)
    {
        if(srcTries.Count == 1)
        {
            orderedVertices = new List<Vector3>(srcTries[0]);
            return;
        }
        orderedVertices = new List<Vector3>();

        HashSet<Vector3> checkVertices = new HashSet<Vector3>();
        //첫번째 라인
        foreach (Vector3[] vs in srcTries)
        {
            checkVertices.Clear();
            checkVertices.Add(vs[0]);
            checkVertices.Add(vs[1]);
            bool sFlag = false;
            foreach (Vector3[] vs2 in srcTries)
            {
                if (vs == vs2)
                    continue;
                int containCount = 0;
                if (checkVertices.Contains(vs2[0])) containCount++;
                if (checkVertices.Contains(vs2[1])) containCount++;
                if (checkVertices.Contains(vs2[2])) containCount++;
                if (containCount == 2)
                    continue;
                sFlag = true;
                break;
            }
            if (sFlag)
            {
                orderedVertices.Add(vs[0]);
                orderedVertices.Add(vs[1]);
                break;
            }
        }


        //삼각형들로부터 중복되지않은 정점수집(정렬안된)
        HashSet<Vector3> allVertices = new HashSet<Vector3>();
        foreach (Vector3[] vs in srcTries)
        {
            allVertices.Add(vs[0]);
            allVertices.Add(vs[1]);
            allVertices.Add(vs[2]);
        }

        allVertices.Remove(orderedVertices[0]);
        allVertices.Remove(orderedVertices[1]);
        Vector3 curCompareVector;


        int sIdx = 0, eIdx = 1;
        while (allVertices.Count > 0)
        {
            curCompareVector = orderedVertices[eIdx] - orderedVertices[sIdx];
            float maxVal = 0.0f;
            Vector3 maxValVertex = allVertices.GetEnumerator().Current;
            foreach (Vector3 v in allVertices)
            {
                Vector3 v1 = v - orderedVertices[0];
                float curDotVal = Vector3.Dot(curCompareVector.normalized, v1.normalized);
                if (curDotVal > maxVal)
                {
                    maxVal = curDotVal;
                    maxValVertex = v;
                }
            }
            allVertices.Remove(maxValVertex);
            orderedVertices.Add(maxValVertex);
            eIdx++;
            sIdx++;
        }
    }
    //인접한 tries 중 Normal이 거의 같은찾기
    public static void GetPlane(out List<Vector3[]> meshTries, Vector3[] targetTriangle, List<Vector3[]> allTries)
    {
        HashSet<Vector3> checkVertices = new HashSet<Vector3>();
        meshTries = new List<Vector3[]>();
        checkVertices.Add(targetTriangle[0]);
        checkVertices.Add(targetTriangle[1]);
        checkVertices.Add(targetTriangle[2]);
        Vector3 N1;
        N1 = Vector3.Cross(targetTriangle[1] - targetTriangle[0], targetTriangle[2] - targetTriangle[1]);
        meshTries.Add(targetTriangle.Clone() as Vector3[]);
        Debug.DrawLine((targetTriangle[0] + targetTriangle[1] + targetTriangle[2]) / 3.0f, (targetTriangle[0] + targetTriangle[1] + targetTriangle[2]) / 3.0f + N1, Color.cyan);

        foreach (Vector3[] vs in allTries)
        {
            int containCount = 0;
            if (checkVertices.Contains(vs[0])) containCount++;
            if (checkVertices.Contains(vs[1])) containCount++;
            if (checkVertices.Contains(vs[2])) containCount++;

            Vector3 N2 = Vector3.Cross(vs[1] - vs[0], vs[2] - vs[1]);
            if (containCount == 2)
            {
                if (Mathf.Abs(1.0f - Vector3.Dot(N1.normalized, N2.normalized)) < 1.0e-3f)
                {
                    Vector3[] vector3s = {
                            vs[0],
                            vs[1],
                            vs[2],
                        };
                    meshTries.Add(vector3s);
                }
            }
        }
    }
    public static void GetPlane(out List<Vector3[]> meshTries, List<Vector3> targetTriangle, List<Vector3[]> allTries)
    {
        GetPlane(out meshTries, targetTriangle.ToArray(), allTries);
    }

    //allVec중에서 targetVec과 가장 가까운 Vector3를 closestVec에 담아줍니다.
    public static void GetClosestVertex(out Vector3 closestVec, Vector3 targetVec, Vector3[] allVec)
    {
        int closestVecIdx = 0;
        float minDist = Vector3.Distance(targetVec, allVec[0]);
        for (int i = 1; i < allVec.Length; ++i)
        {
            float curDist = Vector3.Distance(targetVec, allVec[i]);
            if (minDist > curDist)
            {
                minDist = curDist;
                closestVecIdx = i;
            }
        }
        closestVec = allVec[closestVecIdx];
    }
    public static void GetClosestVertex(out Vector3 closestVec, Vector3 targetVec, List<Vector3> allVec)
    {
        GetClosestVertex(out closestVec, targetVec, allVec.ToArray());
    }
}
