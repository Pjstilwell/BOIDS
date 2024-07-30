using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBOIDs : MonoBehaviour
{
    [SerializeField] GameObject boid;
    [SerializeField] bool on;
    public int noOfBirds = 100;
    // Start is called before the first frame update
    void Start()
    {
        if (on) {
            for (int i = 0; i < noOfBirds; i++) {
                Instantiate(boid);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
