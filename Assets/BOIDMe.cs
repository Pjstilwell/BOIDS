using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System;

public class BOIDMe : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject scene;
    [SerializeField] private GameObject player;
    Random random = new Random();
    float speed = 1.2f;
    float sceneTopHeight;
    float sceneBottomHeight;
    float sceneRightWidth;
    float sceneLeftWidth;
    Vector3 rndDirection;
    float rotationSpeed = 10f; 
    float offset = -90f; 
    Vector3 velocity;
    public float radius = 0.4f;
    public float wallCollisionRadius = 0.1f;
    public float repulsionMultiplier = 0.2f;
    public float cohensionMultiplier = 6f;
    public float alignMultiplier = 0.5f;
    public float capSpeedFactor = 0.7f;
    PolygonCollider2D myCollider = new PolygonCollider2D();
    float avoidDistance;
    float lookAhead;

    public LayerMask layersToHit;
    public LayerMask playerLayer;

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
        this.myCollider = this.GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] closeBOIDs = Physics2D.OverlapCircleAll(transform.position, radius, layersToHit);
        transform.position += velocity * Time.deltaTime;
        shiftBOID();
        closeBOIDs = removeMeFromArray(closeBOIDs);
        RotateTowardsTarget();
        if(closeBOIDs.Length > 0) {
            cohesion(closeBOIDs);
            alignment(closeBOIDs);
            repulsion(closeBOIDs);
        }
        avoidWalls();
        avoidPlayer();
        capVelocity();
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

    public Collider2D[] removeMeFromArray(Collider2D[] array) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i].GetComponent<PolygonCollider2D>() == this.myCollider) {
                var list = new List<Collider2D>(array);
                list.RemoveAt(i);
                return list.ToArray();
            }
        }
        return array;
    }

    public void capVelocity() {
        if (velocity.magnitude > speed) {
            Vector3 normalised = velocity;
            normalised.Normalize();
            velocity = Vector3.Lerp(velocity, normalised * speed, capSpeedFactor);
            // velocity = normalised * speed;
        }
    }

    public void avoidWalls() {
        Ray ray = new Ray(transform.position, velocity.normalized);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, wallCollisionRadius, layersToHit)) {
            this.sprite.color = Color.red;
            Vector3 rotatedVel = Quaternion.AngleAxis(180 * (hit.distance / wallCollisionRadius), Vector3.forward) * velocity;
            velocity += rotatedVel;

        } else {
            this.sprite.color = Color.black;
        }
    }

    public void alignment(Collider2D[] closeBOIDs) {
        var averageDirection = new Vector3();
        for (int i = 0; i < closeBOIDs.Length; i++) {
            averageDirection += new Vector3(closeBOIDs[i].GetComponent<Rigidbody2D>().velocity.x, closeBOIDs[i].GetComponent<Rigidbody2D>().velocity.y, 0);
        }
        averageDirection /= closeBOIDs.Length;
        velocity += Vector3.Lerp(velocity, averageDirection, Time.deltaTime) * alignMultiplier;
    }

    public void cohesion(Collider2D[] closeBOIDs) {
        var averagePosition = new Vector3();
        for (int i = 0; i < closeBOIDs.Length; i++) {
            averagePosition += closeBOIDs[i].transform.position;
        }
        averagePosition /= closeBOIDs.Length;
        var weight = (averagePosition - transform.position).magnitude / radius;
        Vector3 zero = new Vector3(0,0,0);
        velocity += Vector3.Lerp(zero, averagePosition - transform.position, weight) * cohensionMultiplier;
    }

    public void repulsion(Collider2D[] closeBOIDs) {
        var totalRepulsion = new Vector3();
        for (int i = 0; i < closeBOIDs.Length; i++) {
            Vector3 repulsion = transform.position - closeBOIDs[i].transform.position;
            if (repulsion.magnitude != 0) {
                Vector3 weightedRepulsion = repulsion / repulsion.magnitude;
                totalRepulsion += weightedRepulsion;
            }
        }
        velocity += totalRepulsion * repulsionMultiplier;
    }

    public void avoidPlayer() {
        if ((player.transform.position - transform.position).magnitude < radius) {
            Vector3 swimAway = player.transform.position - transform.position;
            velocity += -swimAway * cohensionMultiplier;
            // Debug.Log("close");
        }
    }
}
