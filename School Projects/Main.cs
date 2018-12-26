using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This is the main class from a school project I completed during my 2018 fall semster
The project was to use Unity to re-create the old Asteriod game
We were not allowed to use any of Unity's built in tools for movement, collisions, or tasks such as that
*/

/*this it the main class
all the methods are called here and it is attached to the scene manager
it checks for collisions
it calls the move methods for all the objects
it has timers so it knows when new objects can/are spawned
it runs the game
*/
public class Main : MonoBehaviour {

    //variables
    private List<GameObject> asteriodList = new List<GameObject>();
    public List<GameObject> bulletList = new List<GameObject>();
    public List<GameObject> asteriodSpecList = new List<GameObject>();
    float timerSpawn = 0.0f;
    float timerShoot = 0.0f;
    float timeToShoot = 1.0f;
    float timeToSpawn = 1.5f;
    float timerSpawnSpec = 0.0f;
    float timeToSpawnSpec = 5.0f;
    float countdown; 
    private CollisionDetect collideClass;
    public GameObject[] asteriodChoices = new GameObject[5];
    private int rndm;
    public GameObject ship;
    public Vehicle shipClass;
    private GameObject collidedAsteriod;
    int score = 0;
    public Camera cam;
    float camWidth;
    float camHeight;
    bool immune;
    float immuneTimer = 0.0f;
    float timeToImmune = 2.0f;
    public Texture2D shipSkin;
    public GameObject specAsteroid;


    // Use this for initialization
    void Start ()
    {
        collideClass = new CollisionDetect();
        camHeight = cam.orthographicSize * 2f;
        camWidth = camHeight * cam.aspect;
    }
	
	// Update is called once per frame
	void Update () {

        //Debug.Log(shipClass.lives);

        //loop for just checking collsions between the ship and asteriods
        foreach(GameObject a in asteriodList)
        {
            //checking to see if the ship collides with any asteriods
            if (collideClass.CircleCollision(ship, a))
            {
                //taking a life away if it still has extra lives
                if (shipClass.lives > 0 && !immune)
                {
                    //temporary saftey from getting hurt again
                    immune = true;
                    shipClass.lives--;
                    //teleporting you to the origin if you get hit
                    shipClass.ResetPos();
                }
                else if(shipClass.lives <= 0 && !immune)
                {
                    ship.SetActive(false);
                }
            } 
        }

        foreach (GameObject a in asteriodSpecList)
        {
            //checking to see if the ship collides with any special asteriods
            if (collideClass.CircleCollision(ship, a))
            {
                //taking a life away if it still has extra lives
                if (shipClass.lives > 0 && !immune)
                {
                    //temporary saftey from getting hurt again
                    immune = true;
                    shipClass.lives--;
                    //teleporting you to the origin if you get hit
                    shipClass.ResetPos();
                }
                else if (shipClass.lives <= 0 && !immune)
                {
                    ship.SetActive(false);
                }
            }
        }

        //checking to see if immune period is over
        if (immune)
        {
            //used to show the ship is temp immune
            ship.GetComponent<SpriteRenderer>().color = Color.green;
            if(immuneTimer >= timeToImmune)
            {
                immune = false;
                immuneTimer = 0.0f;
            }
        }
        else
        {
            ship.GetComponent<SpriteRenderer>().color = Color.white;
        }
        if (ship.activeInHierarchy)
        {

            //loop for moving asteriods and other functions
            foreach (GameObject s in asteriodList)
            {
                Asteriod temp = s.GetComponent<Asteriod>();
                temp.Move();
                //Debug.Log(temp.name);
                //Debug.Log(temp.position);
            }
            foreach (GameObject b in bulletList)
            {
                Bullet temp = b.GetComponent<Bullet>();
                temp.Move();
            }
            foreach (GameObject s in asteriodSpecList)
            {
                SpecialAsteroid temp = s.GetComponent<SpecialAsteroid>();
                temp.Move(ship);
            }
        }

        //checking collision between bullet and asteriods
        for (int i = 0; i < bulletList.Count; i++)
        {
            for(int x = 0; x < asteriodList.Count; x++)
            {
                
                if(collideClass.PointCollision( bulletList[i], asteriodList[x]))
                {
                    Destroy(bulletList[i]);
                    bulletList.RemoveAt(i);
                    //If it is the first time the asteriod is shot it runs this
                    if (asteriodList[x].GetComponent<Asteriod>().asteriodLevel == 1)
                    {
                        GameObject[] temp = asteriodList[x].GetComponent<Asteriod>().BreakApart(asteriodList[x]);
                        asteriodList.RemoveAt(x);
                        foreach(GameObject g in temp)
                        {
                            asteriodList.Add(g);
                        }
                        score += 20;
                    }
                    //If it is the second time the asteriod is shot it runs this
                    else if (asteriodList[x].GetComponent<Asteriod>().asteriodLevel == 2)
                    {
                        GameObject[] temp = asteriodList[x].GetComponent<Asteriod>().BreakApart(asteriodList[x]);
                        asteriodList.RemoveAt(x);
                        foreach (GameObject g in temp)
                        {
                            asteriodList.Add(g);
                        }
                        score += 50;
                    }
                    //If it is the third time the asteriod is shot, it destroys it
                    else
                    {
                        Destroy(asteriodList[x]);
                        asteriodList.RemoveAt(x);
                        score += 90;
                    }
                }
            }
        }

        //checking to see if the player shoot the special asteroid
        for (int i = 0; i < bulletList.Count; i++)
        {
            for (int x = 0; x < asteriodSpecList.Count; x++)
            {
                if (collideClass.PointCollision(bulletList[i], asteriodSpecList[x]))
                {
                    Destroy(bulletList[i]);
                    bulletList.RemoveAt(i);
                    Destroy(asteriodSpecList[x]);
                    asteriodSpecList.RemoveAt(x);
                    score += 100;
                }
            }
        }

                    //getting rid of asteroids that are no longer on screen
                    for (int x = 0; x < asteriodList.Count; x++)
        {
            if(asteriodList[x].transform.position.x > camWidth / 2 + 3 || asteriodList[x].transform.position.x < camWidth / -2 - 3 
                || asteriodList[x].transform.position.y > camHeight / 2 + 3 || asteriodList[x].transform.position.y < camHeight /-2 - 3)
            {
                Destroy(asteriodList[x]);
                asteriodList.RemoveAt(x);
            }
        }

        //getting rid of bullets as they go off screen
        for (int i = 0; i < bulletList.Count; i++)
        {
            if (bulletList[i].transform.position.x > camWidth / 2 + 1 || bulletList[i].transform.position.x < camWidth / -2 - 1
                || bulletList[i].transform.position.y > camHeight / 2 + 1 || bulletList[i].transform.position.y < camHeight / -2 - 1)
            {
                Destroy(bulletList[i]);
                bulletList.RemoveAt(i);
            }
        }
        //Spawing asteriods as long as the player is still alive
        if (ship.activeInHierarchy)
        {
            if (timerSpawn >= timeToSpawn)
            {
                Spawn();
                timerSpawn = 0;
            }
            if (timerSpawnSpec >= timeToSpawnSpec)
            {
                SpawnSpecial();
                timerSpawnSpec = 0;
            }

        
            //methods for the vehicle class
            shipClass.RotateVehicle();

            shipClass.Drive();

            shipClass.SetTransform();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (timerShoot >= timeToShoot)
                {
                    //Debug.Log("Calling Shoot");
                    shipClass.Shoot();
                    timerShoot = 0.0f;
                }
            }
        }

    }


    //using this to spawn asteriods based on a timer and limit the speed of firing
    private void FixedUpdate()
    {
        if (Time.time >= countdown)
        {
            //Debug.Log(Time.time + ">=" + countdown);
            // Change the next update (current second+1)
            countdown = Mathf.FloorToInt(Time.time) + 1;
            timerSpawn++;
            timerShoot++;
            timerSpawnSpec++;
            if (immune)
            {
                immuneTimer++;
            }
        }
        
    }


    
    

    //making a method to spawn in asteriods with a random apperance
    public void Spawn()
    {
        Vector3 pos = new Vector3(0,0,0);
        float rndmSpot;
        //Debug.Log("Spawning asteriod");
        rndm = Random.Range(0, 5);
        GameObject temp = Instantiate(asteriodChoices[rndm]);
        Asteriod asteroidClass = temp.GetComponent<Asteriod>();
        rndm = Random.Range(0, 4);
        //Deciding which side of the screen the asteriod will come from
        if(rndm == 0)
        {
            rndmSpot = Random.Range(camWidth / -2, camWidth / 2);
            pos = new Vector3(rndmSpot, camHeight / 2 + 2,0);
        }
        else if(rndm == 1)
        {
            rndmSpot = Random.Range(camWidth / -2, camWidth / 2);
            pos = new Vector3(rndmSpot, camHeight / -2 - 2,0);
        }
        else if(rndm == 2)
        {
            rndmSpot = Random.Range(camHeight / -2, camHeight / 2);
            pos = new Vector3(camWidth / 2 + 2 , rndmSpot,0);
        }
        else if(rndm == 3)
        {
            rndmSpot = Random.Range(camHeight / -2, camHeight / 2);
            pos = new Vector3(camWidth / -2 - 2, rndmSpot, 0);
        }
        asteroidClass.SetUp(pos);
        asteriodList.Add(temp);
    }

    //Method for spawning the special seeking asteriod
    public void SpawnSpecial()
    {
        Vector3 pos = new Vector3(0, 0, 0);
        float rndmSpot;
        //Debug.Log("Spawning asteriod");
        GameObject temp = Instantiate(specAsteroid);
        SpecialAsteroid asteroidClass = temp.GetComponent<SpecialAsteroid>();
        rndm = Random.Range(0, 4);
        //decding which side of the screen to spawn the asteriod on
        if (rndm == 0)
        {
            rndmSpot = Random.Range(camWidth / -2, camWidth / 2);
            pos = new Vector3(rndmSpot, camHeight / 2 + 2, 0);
        }
        else if (rndm == 1)
        {
            rndmSpot = Random.Range(camWidth / -2, camWidth / 2);
            pos = new Vector3(rndmSpot, camHeight / -2 - 2, 0);
        }
        else if (rndm == 2)
        {
            rndmSpot = Random.Range(camHeight / -2, camHeight / 2);
            pos = new Vector3(camWidth / 2 + 2, rndmSpot, 0);
        }
        else if (rndm == 3)
        {
            rndmSpot = Random.Range(camHeight / -2, camHeight / 2);
            pos = new Vector3(camWidth / -2 - 2, rndmSpot, 0);
        }
        asteroidClass.SetUp(pos);
        asteriodSpecList.Add(temp);
    }


    //making the gui
    private void OnGUI()
    {
        int distanceBetween = 50;

        GUI.skin.label.fontSize = 50;

        GUI.Label(new Rect(90, 20, 200, 70), score.ToString());
        //drawing lives
        for (int i = 0; i < shipClass.lives; i++)
        {
            GUI.Label(new Rect(distanceBetween, 70, 50, 50), shipSkin);
            distanceBetween += 50;
        }

        if (ship.activeInHierarchy == false)
        {
            GUI.skin.label.fontSize = 100;
            GUI.Label(new Rect(250, 150, 1000, 500), "Game Over");
        }

    }
}
