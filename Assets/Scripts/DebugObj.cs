using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugObj : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(ImDie());
    }
    IEnumerator ImDie()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        Destroy(gameObject);
    }
}
