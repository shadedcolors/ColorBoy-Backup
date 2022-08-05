using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public SpriteRenderer sprite;

    //Get Text Prefab
    [SerializeField] private GameObject textPrefab;

    //Get Health Text
    TMP_Text healthText;

    //Set colors
    Color32 blueColor = new Color32(121, 171, 209, 255);
    Color32 redColor = new Color32(255, 190, 92, 255);
    Color32 greenColor = new Color32(155, 207, 112, 255);

    Color32[] colors = new Color32[3];

    //Set enemy stats
    public int enemyHealth;
    public int bonusHealth;

    public int EnemyHealth
    {
        get { return enemyHealth; }
        set { enemyHealth = value; }
    }

    public int BonusHealth
    {
        get { return bonusHealth; }
        set { bonusHealth = value; }
    }

    private void Awake()
    {
        EnemyHealth = Random.Range(3, 7);

        BonusHealth = 25;

        //Set Colors
        colors[0] = blueColor;
        colors[1] = redColor;
        colors[2] = greenColor;

        //Draw Correct color
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = colors[Random.Range(0, 3)];

        //get health object
        var textTemp = Instantiate(textPrefab, transform);
        healthText = textTemp.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Show enemy health
        healthText.text = EnemyHealth.ToString();

        //Kill enemy
        if (EnemyHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
