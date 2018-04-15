using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{

    Vector3 target = new Vector3();
    public Camera mainCamera;
    Rigidbody rb;
    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetButton("Fire1"))
        {
            Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            pz.y = 0;

            target = pz;
            
        }
        rb.velocity = Vector3.ClampMagnitude((target - transform.position) * Time.deltaTime * 500, 30);
    }
}
