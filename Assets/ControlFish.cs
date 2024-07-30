using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFish : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    public float playerSpeed = 3f;
    public float rotationAngularSpeed = 1f;
    float rotationSpeed = 10f; 
    float offset = -90f; 
    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector2(0,3);
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKey(KeyCode.LeftArrow)) {
            Vector2 rotatedVel = Quaternion.AngleAxis(rotationAngularSpeed, Vector3.forward) * rb.velocity;
            rb.velocity = Vector3.Lerp(rb.velocity, rotatedVel, 0.3f);
        } 
        if (Input.GetKey(KeyCode.RightArrow)) {
            Vector2 rotatedVel = Quaternion.AngleAxis(-rotationAngularSpeed, Vector3.forward) * rb.velocity;
            rb.velocity = Vector3.Lerp(rb.velocity, rotatedVel, 0.3f);
        }
        RotateTowardsTarget();
        rb.velocity = rb.velocity.normalized * 3; 
    }

    public void RotateTowardsTarget() {
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle + offset, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}
