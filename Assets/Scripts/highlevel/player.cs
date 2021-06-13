using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public float Speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    private void Move()
    {

        if (Input.GetKey(KeyCode.UpArrow))
        {

            transform.Translate(Vector3.back * Speed * Time.deltaTime);


        }

        if (Input.GetKey(KeyCode.DownArrow))  // 

        {

            transform.Translate(Vector3.forward * Speed * Time.deltaTime);

        }

        if (Input.GetKey(KeyCode.RightArrow))  // 

        {

            transform.Translate(Vector3.left * Speed * Time.deltaTime);

        }

        if (Input.GetKey(KeyCode.LeftArrow))

        {

            transform.Translate(Vector3.right * Speed * Time.deltaTime);

        }

    }


}
