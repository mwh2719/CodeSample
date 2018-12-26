using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *Class to hold all the different movment methods
 * Abstract, humans and zombies inherit from it
 * Keeps GameObjects from falling off the terrian
 * 
 * */

public abstract class Vehicle : MonoBehaviour {

    //variables


    // Vectors necessary for force-based movement
    public Vector3 vehiclePosition;
    public Vector3 acceleration;
    public Vector3 direction;
    public Vector3 velocity;


    public Terrain ground;

    private Vector3 center = new Vector3(25, 0, 25);

    
    //Variables for all the lists coming from the scene manager
    public List<GameObject> humans;
    public List<GameObject> zombies;
    public List<GameObject> obstacles;
    public List<GameObject> healing;
    public Manager sceneMang;

    //Used for obstacle avoidance
    float radius = 4;
    float safeDistance = 5;


    // Floats
    public float mass;
    public float maxSpeed;


    //Variables for debugging lines
    protected GameObject future;
    protected GameObject futurePos;
    private bool clicked = false;
    private bool debug = false;

    float wanderAngle = 1.0f;


    // Use this for initialization
    protected void Start()
    {

        sceneMang = GameObject.Find("SceneManager").GetComponent<Manager>();
        future = sceneMang.future;

        //setting up list of obstacles for later use
        obstacles = sceneMang.obstacles;

    }

    // Update is called once per frame
    protected void Update()
    {


        //setting the bool to the opposite value when d is pressed
        if (Input.GetKeyDown(KeyCode.D))
        {

            if (clicked)
            {
                futurePos.SetActive(!debug);
                debug = !debug;
            }
            else
            {
                futurePos = Instantiate(future, vehiclePosition, Quaternion.Euler(Vector3.zero));
                clicked = true;
                debug = !debug;
            }
        }

        //Making game objects the represent the future position of the vehicle
        if (debug)
        {
            futurePos.transform.position = this.transform.position + this.velocity;
        }
        


        //keeping the human zombie lists updated
        humans = sceneMang.humans;
        zombies = sceneMang.zombies;
        healing = sceneMang.healing;

        

        //checking the vehicles against all obstacles
        for (int i = 0; i < obstacles.Count; i++)
        {
            ApplyForce(ClassObstacleAvoidance(obstacles[i]));
        }

        //Calling the movement methods
        CalcSteeringForce();
        Bounce();
        velocity += acceleration * Time.deltaTime;
        Vector3.ClampMagnitude(velocity, maxSpeed);
        vehiclePosition += velocity * Time.deltaTime;
        
        //Moving the vehicle and getting variables reset for next update
        direction = velocity.normalized;
        acceleration = Vector3.zero;
        transform.position = vehiclePosition;
    }

    //method used to make vehicles wander randomly
    /*
     * Wandering is accomplished by making an imaginary circle a little ways in front of the vehicle 
     * and then choosing a point on that circle for it to seek and changing it slighlty each time the method is called
     * */
    protected Vector3 Wandering()
    {
        //setting up how far out the circle is
        Vector3 circleCenter = velocity;
        //if there is no velocity, just giving it a small push to get started
        if(circleCenter == Vector3.zero)
        {
            circleCenter = new Vector3(1, 0, 1);
        }
        circleCenter.Normalize();
        circleCenter *= 1;


        //Setting up the circle size
        Vector3 displacment = new Vector3(0, 0, -1);
        displacment.Normalize();
        displacment *= 2;

        //Choosing a random point on the circle
        displacment.x = Mathf.Cos(wanderAngle) * displacment.magnitude;
        displacment.z = Mathf.Sin(wanderAngle) * displacment.magnitude;

        wanderAngle += Random.Range(-.5f, .5f);

        //returning the wander force to be used
        Vector3 wanderForce = circleCenter + displacment;

        wanderForce *= maxSpeed;

        wanderForce -= velocity;

        return wanderForce;
    }

    //abstract method used to make sure the characters stay seperated from each other
    protected abstract Vector3 Separation();

     
    //Method to have vehicles detect obstacles in front of them and avoid them
    protected  Vector3 ClassObstacleAvoidance(GameObject obst)
    {
        Vector3 vecCenter = obst.transform.position - this.transform.position;
        float dotRight = Vector3.Dot(vecCenter, transform.right);
        float dotForward = Vector3.Dot(vecCenter, transform.forward);
        float radiSum = obst.GetComponent<Obstacle>().radius + radius;//variable to hold vehicle radius

        //Checiking if the obstacle is behind it
        if(dotForward < 0)
        {
            return Vector3.zero;
        }

        //Checking to see if the obstacle is too close
        if (vecCenter.sqrMagnitude > (safeDistance * safeDistance)) //variable to hold distance when vehicle should start avoiding)
        {
            return Vector3.zero;
        }

        //Checking to see if the object is in the path of the vehicle
        if(Mathf.Abs(dotRight) > radiSum)
        {
            return Vector3.zero;
        }

        //Checking to see if the obstcale is on the right or left then adding a force in the other direction
        Vector3 desiredVelocity = Vector3.zero;
        //Checking the left side
        if(dotRight < 0)
        {
            desiredVelocity = transform.right * maxSpeed;
            desiredVelocity *= 1 / vecCenter.magnitude;

        }
        //Checking the right side
        else
        {
            desiredVelocity = -transform.right * maxSpeed;
            desiredVelocity *= 1 / vecCenter.magnitude;
        }

        desiredVelocity *= 7;

        return desiredVelocity - velocity;
    }
    
    

    // ApplyForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyForce(Vector3 force)
    {
        acceleration += force / mass;
    }

    // ApplyForce
    // Receive an incoming force, divide by mass, and apply to the cumulative accel vector
    public void ApplyGravityForce(Vector3 force)
    {
        acceleration += force;
    }


    // SEEK METHOD
    // All Vehicles have the knowledge of how to seek
    // They just may not be calling it all the time
    /// <summary>
    /// Seek
    /// </summary>
    /// <param name="targetPosition">Vector3 position of desired target</param>
    /// <returns>Steering force calculated to seek the desired target</returns>
    public Vector3 Seek(Vector3 targetPosition)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = targetPosition - vehiclePosition;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 seekingForce = desiredVelocity - velocity;

        // Step 4: Return force
        return seekingForce;
    }

    /// <summary>
    /// Overloaded Seek
    /// </summary>
    /// <param name="target">GameObject of the target</param>
    /// <returns>Steering force calculated to seek the desired target</returns>
    public Vector3 Seek(GameObject target)
    {
        return Seek(target.transform.position);
    }

    //making a method to pursue the vehicles future position rather than its current one
    public Vector3 Pursue(GameObject target)
    {
        // Step 1: Find DV (desired velocity)
        // TargetPos - CurrentPos
        Vector3 desiredVelocity = ((target.transform.position + target.GetComponent<Vehicle>().velocity) - vehiclePosition) * 2;

        // Step 2: Scale vel to max speed
        // desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;

        // Step 3:  Calculate seeking steering force
        Vector3 seekingForce = desiredVelocity - velocity;

        // Step 4: Return force
        return seekingForce;
    }

    

    //Creating a flee method
    public Vector3 Flee(Vector3 targetPosition)
    {

        Vector3 desiredVelocity = vehiclePosition - targetPosition;


        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;


        Vector3 fleeingForce = desiredVelocity - velocity;

        return fleeingForce;
    }

    
    //Method that uses a gameobject by plugging it in to the other flee method
    public Vector3 Flee(GameObject target)
    {
        return Flee(target.transform.position);
    }

    //method to evade the vehicles future position
    public Vector3 Evade(GameObject target)
    {

        Vector3 desiredVelocity = vehiclePosition - (target.transform.position + target.GetComponent<Vehicle>().velocity);


        desiredVelocity.Normalize();
        desiredVelocity = desiredVelocity * maxSpeed;


        Vector3 fleeingForce = desiredVelocity - velocity;

        return fleeingForce;
    }


    //method to make the steering force for the object
    public abstract void CalcSteeringForce();

    //Method to keep all the vehicles from falling off the terrian edges
    //If they get to close to an edge they will simply seek the center again
    public void Bounce()
    {
        if (vehiclePosition.x - 5 < 0)
        {
            ApplyForce(Seek(center) * 2);
        }

        if (vehiclePosition.x + 5 > 50)
        {
            ApplyForce(Seek(center) * 2);
        }

        if (vehiclePosition.z - 5 < 0)
        {
            ApplyForce(Seek(center) * 2);
        }

        if (vehiclePosition.z + 5 > 50)
        {
            ApplyForce(Seek(center) * 2);
        }
    }
}
