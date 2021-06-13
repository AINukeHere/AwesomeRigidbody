using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public static SimulationController instance { get; private set; }
    [SerializeField] private Material[] mats;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public GameObject awesomeRigidObj;
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Camera mainCamera = Camera.main;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo, 1000.0f))
            {
                Vector3 spawnPos = hitInfo.point;
                spawnPos.y = 50.0f;

                Instantiate(awesomeRigidObj, spawnPos, Quaternion.Euler(-30,-90,-50));
            }
        }
    }
    public Material GetRandomMat()
    {
        int nansu = Random.Range(0, mats.Length);
        return mats[nansu];
    }
}
