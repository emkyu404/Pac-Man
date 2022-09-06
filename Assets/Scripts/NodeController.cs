using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{

    public GameManager gameManager;
    public bool canMoveLeft = false;
    public bool canMoveRight = false;
    public bool canMoveUp = false;
    public bool canMoveDown = false;

    public GameObject nodeLeft;
    public GameObject nodeRight;
    public GameObject nodeUp;
    public GameObject nodeDown;

    public bool isWarpRightNode;
    public bool isWarpLeftNode;

    //if the node contains a pellet when the game starts
    public bool isPelletNode = true;

    //if the node still has a pellet
    public bool hasPellet = false;

    public bool isGhostStartingNode = false;

    public SpriteRenderer pelletSprite;

    public bool isSideNode = false;

    public bool isPowerPellet = false;

    public float powerPelletBlinkingTimer = 0;



    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if(transform.childCount > 0)
        {
            gameManager.GotPelletFromNodeController(this);
            hasPellet = true;
            isPelletNode = true;
            pelletSprite = GetComponentInChildren<SpriteRenderer>();
        }

        RaycastHit2D[] hitsDown;
        //shoot raycast (line) going down
        hitsDown = Physics2D.RaycastAll(transform.position, -Vector2.up);

        //loop through all of the game object that the raycast hit
        for(int i = 0; i < hitsDown.Length; i++)
        {
            float distance = Mathf.Abs(hitsDown[i].point.y - transform.position.y);
            if(distance < 0.4f && hitsDown[i].collider.tag == "Node")
            {
                canMoveDown = true;
                nodeDown = hitsDown[i].collider.gameObject;
            }
        }

        RaycastHit2D[] hitsUp;
        //shoot raycast (line) going up
        hitsUp = Physics2D.RaycastAll(transform.position, Vector2.up);

        //loop through all of the game object that the raycast hit
        for (int i = 0; i < hitsUp.Length; i++)
        {
            float distance = Mathf.Abs(hitsUp[i].point.y - transform.position.y);
            if (distance < 0.4f && hitsUp[i].collider.tag == "Node")
            {
                canMoveUp = true;
                nodeUp = hitsUp[i].collider.gameObject;
            }
        }

        RaycastHit2D[] hitsRight;
        //shoot raycast (line) going right
        hitsRight = Physics2D.RaycastAll(transform.position, Vector2.right);

        //loop through all of the game object that the raycast hit
        for (int i = 0; i < hitsRight.Length; i++)
        {
            float distance = Mathf.Abs(hitsRight[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsRight[i].collider.tag == "Node")
            {
                canMoveRight = true;
                nodeRight = hitsRight[i].collider.gameObject;
            }
        }

        RaycastHit2D[] hitsLeft;
        //shoot raycast (line) going left
        hitsLeft = Physics2D.RaycastAll(transform.position, -Vector2.right);

        //loop through all of the game object that the raycast hit
        for (int i = 0; i < hitsLeft.Length; i++)
        {
            float distance = Mathf.Abs(hitsLeft[i].point.x - transform.position.x);
            if (distance < 0.4f && hitsLeft[i].collider.tag == "Node")
            {
                canMoveLeft = true;
                nodeLeft = hitsLeft[i].collider.gameObject;
            }
        }

        if (isGhostStartingNode)
        {
            canMoveDown = true;
            nodeDown = gameManager.ghostNodeCenter;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameIsRunning)
        {
            return;
        }

        if(isPowerPellet && hasPellet)
        {
            powerPelletBlinkingTimer += Time.deltaTime;
            if(powerPelletBlinkingTimer  >= 0.2f)
            {
                powerPelletBlinkingTimer = 0;
                pelletSprite.enabled = !pelletSprite.enabled;
            }
        }
    }

    public GameObject GetNodeFromDirection(string direction)
    {
        if(direction == "left" && canMoveLeft)
        {
            return nodeLeft;
        }
        else if (direction == "right" && canMoveRight)
        {
            return nodeRight;
        }
        else if (direction == "up" && canMoveUp)
        {
            return nodeUp;
        }
        else if (direction == "down" && canMoveDown)
        {
            return nodeDown;
        }
        else
        {
            return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(collision.tag == "Player" && hasPellet)
        {
            hasPellet = false;
            pelletSprite.enabled = false;
            StartCoroutine(gameManager.CollectedPellet(this));

        }
    }

    public void RespawnPellet()
    {
        if (isPelletNode)
        {
            hasPellet = true;
            pelletSprite.enabled = true;
        }
    }
}
