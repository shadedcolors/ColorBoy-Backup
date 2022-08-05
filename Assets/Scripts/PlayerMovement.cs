using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    SpriteRenderer sprite;

    //Set colors
    Color32 blueColor = new Color32(121, 171, 209, 255);
    Color32 redColor = new Color32(255, 190, 92, 255);
    Color32 greenColor = new Color32(155, 207, 112, 255);

    //Swiping Variables
    private Vector2 startSwipePos; //the starting position of where the finger started swiping
    public int pixelDistToDetect = 25; //How many pixels in a direction is needed for the swipe to move the player
    private bool fingerDown; //Check if the finger is down

    //Set Tilemap
    Grid grid;
    Tilemap tilemap;

    //Set Camera
    GameObject camera;
    Vector3 offset = new Vector3(0, 0, -1);

    //Set Layermask for raycasting
    [SerializeField] private LayerMask layerMask;

    //Get Text Prefab
    [SerializeField] private GameObject textPrefab;

    //Get Health Text
    TMP_Text healthText;

    //Set Player Health
    int playerHealth = 40;

    //Player Effects
    bool colorOrderNormal = true;
    bool timer = false;
    int timerCount = 0;
    bool nullifyNextTile = false;

    public int PlayerHealth
    {
        get { return playerHealth; }
        set { playerHealth = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get all objects/components
        sprite = GetComponent<SpriteRenderer>();

        grid = GameObject.Find("Grid").GetComponent<Grid>();

        tilemap = GameObject.Find("Tilemap Base").GetComponent<Tilemap>();

        camera = GameObject.Find("Main Camera");

        Vector3 startPos = GameObject.Find("Start Position").transform.position;
        transform.position = startPos;

        //Set initial color to Blue
        sprite.color = blueColor;

        //get health object
        var textTemp = Instantiate(textPrefab, transform);
        healthText = textTemp.GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //Draw health
        healthText.text = playerHealth.ToString();

        //check for player death
        if (playerHealth <= 0)
        {
            SceneManager.LoadScene(0);
        }

        //Camera follow player
        camera.transform.position = transform.position + offset;

        //Check for initial finger touch
        if (!fingerDown && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            startSwipePos = Input.touches[0].position;
            fingerDown = true;
        }

        //If touching with your finger...
        if (fingerDown)
        {
            //Swipe Up
            if (Input.touches[0].position.y >= startSwipePos.y + pixelDistToDetect)
            {
                fingerDown = false;
                Move(new Vector3(0, 1, 0));
            }
            //Swipe Down
            else if (Input.touches[0].position.y <= startSwipePos.y - pixelDistToDetect)
            {
                fingerDown = false;
                Move(new Vector3(0, -1, 0));
            }
            //Swipe Left
            else if (Input.touches[0].position.x <= startSwipePos.x - pixelDistToDetect)
            {
                fingerDown = false;
                Move(new Vector3(-1, 0, 0));
            }
            //Swipe Right
            else if (Input.touches[0].position.x >= startSwipePos.x + pixelDistToDetect)
            {
                fingerDown = false;
                Move(new Vector3(1, 0, 0));
            }
        }

        //Remove finger from screen
        if (fingerDown && Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended)
        {
            fingerDown = false;
        }

        //Move up arrow
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(new Vector3(0, 1, 0));
        }

        //Move down arrow
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(new Vector3(0, -1, 0));
        }

        //Move left arrow
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(new Vector3(-1, 0, 0));
        }

        //Move right arrow
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(new Vector3(1, 0, 0));
        }
    }

    //Move the player (check for collisions and enemies)
    public void Move(Vector3 direction)
    {
        //Check for collision
        RaycastHit2D hit = Physics2D.Raycast(transform.position + direction, transform.TransformDirection(direction), 0.5f, layerMask);

        Debug.DrawRay(transform.position, transform.TransformDirection(direction), Color.red, 2f, false);
        //Debug.DrawLine(transform.position, transform.position + (direction * 5), Color.red, 10000000f, true);

        Debug.Log(hit.transform.gameObject.tag);

        //Move if able
        if (!hit.transform.gameObject.CompareTag("Wall") && !hit.transform.gameObject.CompareTag("Enemy"))
        {
            transform.position += direction;

            //Cycle through the 3 colors (normal OR reverse)
            if (colorOrderNormal)
            {
                if (sprite.color == blueColor)
                {
                    sprite.color = redColor;
                }
                else if (sprite.color == redColor)
                {
                    sprite.color = greenColor;
                }
                else if (sprite.color == greenColor)
                {
                    sprite.color = blueColor;
                }
            }
            else
            {
                if (sprite.color == greenColor)
                {
                    sprite.color = redColor;
                }
                else if (sprite.color == redColor)
                {
                    sprite.color = blueColor;
                }
                else if (sprite.color == blueColor)
                {
                    sprite.color = greenColor;
                }
            }

            //Count Timer
            if (timer)
            {
                timerCount--;

                //Do timer effect
                if (timerCount <= 0)
                {
                    timer = false;

                    if (sprite.color == blueColor)
                    {
                        playerHealth += 10;
                        Debug.Log("Healed");
                    }
                    else if (sprite.color == redColor)
                    {
                        playerHealth -= 10;
                        Debug.Log("Hurt");
                    }
                    else if (sprite.color == greenColor)
                    {
                        nullifyNextTile = true;
                        Debug.Log("Nullified");
                    }
                }
            }
        }

        //Attack enemy
        if (hit.transform.gameObject.CompareTag("Enemy"))
        {
            //Hit enemy
            hit.transform.GetComponent<Enemy>().EnemyHealth--;

            //Hit player
            Color enemyColor = hit.transform.GetComponent<Enemy>().sprite.color;
            //If enemy is the same color...
            if (sprite.color == enemyColor)
            {
                //Hit player for normal damage
                playerHealth -= 2;
            }
            //If player beats enemy color...
            else if ((sprite.color == blueColor && enemyColor == redColor) || (sprite.color == redColor && enemyColor == greenColor) || (sprite.color == greenColor && enemyColor == blueColor))
            {
                //Hit player for less damage
                playerHealth -= 1;
            }
            //If player loses to enemy color...
            else if ((sprite.color == blueColor && enemyColor == greenColor) || (sprite.color == greenColor && enemyColor == redColor) || (sprite.color == redColor && enemyColor == blueColor))
            {
                //Hit player for extra damage
                playerHealth -= 3;
            }

            //Check for enemy death
            if (hit.transform.GetComponent<Enemy>().EnemyHealth <= 0)
            {
                PlayerHealth += hit.transform.GetComponent<Enemy>().BonusHealth;
            }
        }

        //Tile Effects
        if (hit.transform.gameObject.CompareTag("Floor"))
        {
            //Check for nulliying the next tile
            if (nullifyNextTile)
            {
                nullifyNextTile = false;
            }
            //Otherwise, do the normal effect
            else
            {
                //Reverse Color Order Effect
                if (hit.transform.gameObject.name == "ReverseColorOrderTile")
                {
                    colorOrderNormal = !colorOrderNormal;
                }
                //Damage Player Effect
                else if (hit.transform.gameObject.name == "DamageTile")
                {
                    playerHealth--;
                }
                //Change Color To Red Effect
                else if (hit.transform.gameObject.name == "ChangeColorToRedTile")
                {
                    sprite.color = redColor;
                }
                //Change Color To Green Effect
                else if (hit.transform.gameObject.name == "ChangeColorToGreenTile")
                {
                    sprite.color = greenColor;
                }
                //Change Color To Red Effect
                else if (hit.transform.gameObject.name == "ChangeColorToBlueTile")
                {
                    sprite.color = blueColor;
                }
                //Heal If Green Effect
                else if (hit.transform.gameObject.name == "HealGreenTile")
                {
                    if (sprite.color == greenColor)
                    {
                        playerHealth += 10;
                    }
                }
                //Heal If Red Effect
                else if (hit.transform.gameObject.name == "HealRedTile")
                {
                    if (sprite.color == redColor)
                    {
                        playerHealth += 10;
                    }
                }
                //Heal If Blue Effect
                else if (hit.transform.gameObject.name == "HealBlueTile")
                {
                    if (sprite.color == blueColor)
                    {
                        playerHealth += 10;
                    }
                }
                //Damage If Not Green Effect
                else if (hit.transform.gameObject.name == "DamageNotGreenTile")
                {
                    if (sprite.color != greenColor)
                    {
                        playerHealth -= 10;
                    }
                }
                //Damage If Not Red Effect
                else if (hit.transform.gameObject.name == "DamageNotRedTile")
                {
                    if (sprite.color != redColor)
                    {
                        playerHealth -= 10;
                    }
                }
                //Damage If Not Blue Effect
                else if (hit.transform.gameObject.name == "DamageNotBlueTile")
                {
                    if (sprite.color != blueColor)
                    {
                        playerHealth -= 10;
                    }
                }
                //Timer Effect
                else if (hit.transform.gameObject.name == "TimerTile")
                {
                    timer = true;
                    timerCount = 4;
                }
            }
        }
    }
}
