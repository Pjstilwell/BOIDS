using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BOIDMeTest : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject scene;
    Random random = new Random();
    float speed = 1f;
    float sceneTopHeight;
    float sceneBottomHeight;
    float sceneRightWidth;
    float sceneLeftWidth;
    Vector3 rndDirection;
    float rotationSpeed = 20f; 
    float offset = -90f; 
    Vector3 velocity;
    float radius = 2f;
    float repulsionMultiplier = 0.5f;
    Collider2D myCollider = new Collider2D();

    // Start is called before the first frame update
    void Start()
    {
        this.sceneTopHeight = this.scene.transform.position.y + (this.scene.transform.localScale.y/2);
        this.sceneBottomHeight = this.scene.transform.position.y - (this.scene.transform.localScale.y/2);
        this.sceneRightWidth = this.scene.transform.position.x + (this.scene.transform.localScale.x/2);
        this.sceneLeftWidth = this.scene.transform.position.x - (this.scene.transform.localScale.x/2);
        transform.position = new Vector3(GetRandomNumber(this.sceneLeftWidth, this.sceneRightWidth), GetRandomNumber(this.sceneBottomHeight, this.sceneTopHeight), 0);
        this.rndDirection = new Vector3(GetRandomNumber(-1,1), GetRandomNumber(-1,1), 0);
        this.rndDirection.Normalize();
        velocity = this.rndDirection * speed;
        this.myCollider = this.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] closeBOIDs = Physics2D.OverlapCircleAll(transform.position, radius);
        transform.position += velocity * Time.deltaTime;
        capVelocity();
        shiftBOID();
        RotateTowardsTarget();
        if(closeBOIDs.Length > 0) {
            cohesion(closeBOIDs);
            alignment(closeBOIDs);
            // repulsion(closeBOIDs);
        }
    }
    
    public float GetRandomNumber(float minimum, float maximum)
    {
        float rnd = (float)this.random.NextDouble();
        return ((maximum - minimum) * rnd) + minimum; 
    }

    public void shiftBOID() {
        if (transform.position.x > this.sceneRightWidth) {
            transform.position = new Vector3(this.sceneLeftWidth, transform.position.y, 0);
        }

        if (transform.position.x < this.sceneLeftWidth) {
            transform.position = new Vector3(this.sceneRightWidth, transform.position.y, 0);
        }
        
        if (transform.position.y > this.sceneTopHeight) {
            transform.position = new Vector3(transform.position.x, this.sceneBottomHeight, 0);
        }

        if (transform.position.y < this.sceneBottomHeight) {
            transform.position = new Vector3(transform.position.x, this.sceneTopHeight, 0);
        }
    }

    public void RotateTowardsTarget() {
        float angle = Mathf.Atan2(this.velocity.y, this.velocity.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle + offset, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    public void alignment(Collider2D[] closeBOIDs) {
        var averageDirection = new Vector3();
        for (int i = 0; i < closeBOIDs.Length; i++) {
            averageDirection += new Vector3(closeBOIDs[i].GetComponent<Rigidbody2D>().velocity.x, closeBOIDs[i].GetComponent<Rigidbody2D>().velocity.y, 0);
        }
        averageDirection /= closeBOIDs.Length;
        velocity += Vector3.Lerp(velocity, averageDirection, Time.deltaTime);
    }

    public void cohesion(Collider2D[] closeBOIDs) {
        var averagePosition = new Vector3();
        for (int i = 0; i < closeBOIDs.Length; i++) {
            averagePosition += closeBOIDs[i].transform.position;
        }
        averagePosition /= closeBOIDs.Length;
        var weight = (averagePosition - transform.position).magnitude / radius;
        Vector3 zero = new Vector3(0,0,0);
        velocity += Vector3.Lerp(zero, averagePosition - transform.position, weight);
    }

    public void capVelocity() {
        if (velocity.magnitude > speed) {
            Vector3 normalised = velocity;
            normalised.Normalize();
            velocity = normalised * speed;
        }
    }

    public void repulsion(Collider2D[] closeBOIDs) {
        var totalRepulsion = new Vector3();
        for (int i = 0; i < closeBOIDs.Length; i++) {
            Vector3 repulsion = transform.position - closeBOIDs[i].transform.position;
            if (repulsion.magnitude != 0) {
                Vector3 weightedRepulsion = repulsion * (this.repulsionMultiplier / repulsion.magnitude);
        Debug.Log(repulsion.magnitude);
                totalRepulsion += weightedRepulsion;
            }
        }
        velocity += totalRepulsion;
    }
}
