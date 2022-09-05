using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovementController))]
public class EnemyController : MonoBehaviour
{
    public enum GhostNodeStateEnum
    {
        respawning,
        leftNode,
        rightNode,
        centerNode,
        startNode,
        movingInNodes
    }

    public GhostNodeStateEnum ghostNodeState;
    public GhostNodeStateEnum respawnState;
    public GhostNodeStateEnum startState;


    public enum GhostType
    {
        red,
        blue,
        pink,
        orange
    }
    public GhostType ghostType;



    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public bool leftHomeBefore = false;

    public MovementController movementController;

    public GameObject startingNode;

    public bool readyToLeaveHome = false;

    public GameManager gameManager;

    public bool testRespawn = false;

    public bool isFrightened;

    public GameObject[] scatterNodes;

    public int scatterNodeIndex;

    void Awake()
    {
        scatterNodeIndex = 0;
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        movementController = GetComponent<MovementController>();
        if(ghostType == GhostType.red)
        {
            startState = GhostNodeStateEnum.startNode;
            startingNode = ghostNodeStart;
            respawnState = GhostNodeStateEnum.centerNode;
            
        }
        else if (ghostType == GhostType.pink)
        {
            startState = GhostNodeStateEnum.centerNode;
            startingNode = ghostNodeCenter;
            respawnState = GhostNodeStateEnum.centerNode;
        }
        else if (ghostType == GhostType.blue)
        {
            startState = GhostNodeStateEnum.leftNode;
            respawnState = GhostNodeStateEnum.leftNode;
            startingNode = ghostNodeLeft;
        }
        else if (ghostType == GhostType.orange)
        {
            startState = GhostNodeStateEnum.rightNode;
            respawnState = GhostNodeStateEnum.rightNode;
            startingNode = ghostNodeRight;
        }
        

        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;
    }

    public void ReachedCenterOfNode(NodeController nodeController)
    {
        if(ghostNodeState == GhostNodeStateEnum.movingInNodes)
        {
            leftHomeBefore = true;
            //Scatter mode
            if(gameManager.currentGhostMode == GameManager.GhostMode.scatter)
            {
                DetermineGhostScatterModeDirection();
            }
            //Frightened mode
            else if (isFrightened)
            {
                string direction = GetRandomDirection();
                movementController.SetDirection(direction);
            }
            //Chase mode;
            else
            {
                if (ghostType == GhostType.red)
                {
                    DetermineRedGhostDirection();
                }else if(ghostType == GhostType.pink)
                {
                    DeterminePinkGhostDirection();
                }
                else if(ghostType == GhostType.blue)
                {
                    DetermineBlueGhostDirection();
                }
                else if(ghostType == GhostType.orange)
                {
                    DetermineOrangeGhostDirection();
                }
            }
        }
        else if(ghostNodeState == GhostNodeStateEnum.respawning)
        {
            string direction = "";
            direction = getClosestDirection(ghostNodeStart.transform.position);
            //Reached our start node, move to the center node
            if ((transform.position.x == ghostNodeStart.transform.position.x) && (transform.position.y == ghostNodeStart.transform.position.y))
            {
                direction = "down";
            }
            //We reached our  center node, either finish respawn or move to the left right node
            else if(transform.position.x == ghostNodeCenter.transform.position.x && transform.position.y == ghostNodeCenter.transform.position.y)
            {
                if(respawnState == GhostNodeStateEnum.centerNode)
                {
                    ghostNodeState = respawnState;
                    readyToLeaveHome = false;
                    direction = "down"; // if not set to down, will go to side nodes for no reason
                }
                else if(respawnState == GhostNodeStateEnum.leftNode)
                {
                    direction = "left";
                }
                else if(respawnState == GhostNodeStateEnum.rightNode)
                {
                    direction = "right";
                }
                else if (transform.position.x == ghostNodeLeft.transform.position.x && transform.position.y == ghostNodeLeft.transform.position.y 
                    || transform.position.x == ghostNodeRight.transform.position.x && transform.position.y == ghostNodeRight.transform.position.y)
                {
                    ghostNodeState = respawnState;
                    readyToLeaveHome = false;
                    direction = "down";
                }
                // we are in the gameboard still, locate our our start node
                else
                {

                }
            }
            movementController.SetDirection(direction);
        }
        else
        {
            if (readyToLeaveHome)
            {
                //if we are in the left home node, move to the center
                if (ghostNodeState == GhostNodeStateEnum.leftNode) 
                {
                    ghostNodeState = GhostNodeStateEnum.centerNode;
                    movementController.SetDirection("right");
                }
                //if we are in the right home node, move to the center
                else if (ghostNodeState == GhostNodeStateEnum.rightNode)
                {
                    Debug.Log("From right node to center node");
                    ghostNodeState = GhostNodeStateEnum.centerNode;
                    movementController.SetDirection("left");
                }
                //if we are center node, move to start node
                else if(ghostNodeState == GhostNodeStateEnum.centerNode)
                {
                    Debug.Log("From center node to starting node");
                    ghostNodeState = GhostNodeStateEnum.startNode;
                    movementController.SetDirection("up");
                }
                //if we are start node, move around in the game
                else if(ghostNodeState == GhostNodeStateEnum.startNode)
                {
                    ghostNodeState = GhostNodeStateEnum.movingInNodes;
                    movementController.SetDirection("left");
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(gameManager.gameIsRunning == false)
        {
            return;
        }
        if(testRespawn == true)
        {
            ghostNodeState = GhostNodeStateEnum.respawning;
            testRespawn = false;
        }

        if (movementController.currentNode.GetComponent<NodeController>().isSideNode)
        {
            movementController.SetSpeed(1);
        }
        else
        {
            movementController.SetSpeed(3);
        }
    }

    void DetermineGhostScatterModeDirection()
    {
        if(transform.position.x == scatterNodes[scatterNodeIndex].transform.position.x && transform.position.y == scatterNodes[scatterNodeIndex].transform.position.y)
        {
            scatterNodeIndex++;
            scatterNodeIndex %= scatterNodes.Length;
        }
        string direction = getClosestDirection(scatterNodes[scatterNodeIndex].transform.position);
        movementController.SetDirection(direction);
    }

    void DetermineRedGhostDirection()
    {
        string direction = getClosestDirection(gameManager.pacman.transform.position);
        movementController.SetDirection(direction);
    }

    void DeterminePinkGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;

        if(pacmanDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if(pacmanDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if(pacmanDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if(pacmanDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        string direction = getClosestDirection(target);
        movementController.SetDirection(direction);
    }

    void DetermineBlueGhostDirection()
    {
        string pacmanDirection = gameManager.pacman.GetComponent<MovementController>().lastMovingDirection;
        float distanceBetweenNodes = 0.35f;

        Vector2 target = gameManager.pacman.transform.position;

        if (pacmanDirection == "left")
        {
            target.x -= distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "right")
        {
            target.x += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "up")
        {
            target.y += distanceBetweenNodes * 2;
        }
        else if (pacmanDirection == "down")
        {
            target.y -= distanceBetweenNodes * 2;
        }

        GameObject redGhost = gameManager.redGhost;
        float xDistance = target.x - redGhost.transform.position.x;
        float yDistance = target.y - redGhost.transform.position.y;

        Vector2 blueTarget = new Vector2(target.x + xDistance, target.y + yDistance);


        string direction = getClosestDirection(blueTarget);
        movementController.SetDirection(direction);
    }

    void DetermineOrangeGhostDirection()
    {
        float distance = Vector2.Distance(gameManager.pacman.transform.position, transform.position);
        float distanceBetweenNodes = 0.35f;

        if(distance < 0)
        {
            distance *= -1;
        }

        if(distance <= distanceBetweenNodes * 8)
        {
            DetermineRedGhostDirection();
        }
        else
        {
            //Scatter mode logique
            DetermineGhostScatterModeDirection();
        }
    }

    string GetRandomDirection()
    {
        List<string> possibleDirections = new List<string>();
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();

        if(nodeController.name == "Right Warp" || nodeController.name == "Left Warp")
        {
            return movementController.lastMovingDirection;
        }

        if (nodeController.canMoveDown && movementController.lastMovingDirection != "up")
        {
            possibleDirections.Add("down");
        }
        if(nodeController.canMoveUp && movementController.lastMovingDirection != "down")
        {
            possibleDirections.Add("up");
        }
        if (nodeController.canMoveRight && movementController.lastMovingDirection != "left")
        {
            possibleDirections.Add("right");
        }
        if (nodeController.canMoveLeft && movementController.lastMovingDirection != "right")
        {
            possibleDirections.Add("left");
        }

        int randomDirectionIndex = Random.Range(0, possibleDirections.Count - 1);
        
        return possibleDirections[randomDirectionIndex];
    }

    string getClosestDirection(Vector2 target)
    {
        float shortestDistance = 0;
        string lastMovingDirection = movementController.lastMovingDirection;
        string newDirection = "";
        NodeController nodeController = movementController.currentNode.GetComponent<NodeController>();


        // if we can move up and we aren't reversing
        if(nodeController.canMoveUp && lastMovingDirection != "down")
        {
            GameObject nodeUp = nodeController.nodeUp;
            //Get the distance between our top node and pacman
            float distance = Vector2.Distance(nodeUp.transform.position, target);


            //if this is the shortest distance so far, set our direction
            if(distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "up";
            }
        }

        if (nodeController.canMoveDown && lastMovingDirection != "up")
        {
            GameObject nodeDown = nodeController.nodeDown;
            //Get the distance between our top node and pacman
            float distance = Vector2.Distance(nodeDown.transform.position, target);


            //if this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "down";
            }
        }

        if (nodeController.canMoveLeft && lastMovingDirection != "right")
        {
            GameObject nodeLeft = nodeController.nodeLeft;
            //Get the distance between our top node and pacman
            float distance = Vector2.Distance(nodeLeft.transform.position, target);


            //if this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "left";
            }
        }

        if (nodeController.canMoveRight && lastMovingDirection != "left")
        {
            GameObject nodeRight = nodeController.nodeRight;
            //Get the distance between our top node and pacman
            float distance = Vector2.Distance(nodeRight.transform.position, target);


            //if this is the shortest distance so far, set our direction
            if (distance < shortestDistance || shortestDistance == 0)
            {
                shortestDistance = distance;
                newDirection = "right";
            }
        }

        return newDirection;
    }

    public void Setup()
    {
        
        ghostNodeState = startState;
        //Reset our ghost back to their home position
        movementController.currentNode = startingNode;
        transform.position = startingNode.transform.position;

        movementController.direction = "";
        movementController.lastMovingDirection = "";

        //Set their scatter node index back to 0
        scatterNodeIndex = 0;
        //Set isFrightened to false
        isFrightened = false;
        //Set ReadyToLeaveHome to be false if they are blue or pink
        readyToLeaveHome = false;
        leftHomeBefore = false;
        if (ghostType == GhostType.red)
        {
            readyToLeaveHome = true;
            leftHomeBefore = true;
        }else if(ghostType == GhostType.pink)
        {
            readyToLeaveHome = true;
        }
    }

}
