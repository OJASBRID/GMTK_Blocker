using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gm;
    public Vector3 pf;
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        gm.transform.position = pf;
    }
}
