using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject pacman;

    public Image blackBackground;

    public bool newGame;
    public bool clearedLevel;

    public GameObject leftWarpNode;
    public GameObject rightWarpNode;

    public AudioSource siren;

    public AudioSource munch1;
    public AudioSource munch2;
    public int currentMunch;

    public int score;
    public Text scoreText;

    public GameObject ghostNodeLeft;
    public GameObject ghostNodeRight;
    public GameObject ghostNodeCenter;
    public GameObject ghostNodeStart;

    public GameObject redGhost;
    public GameObject blueGhost;
    public GameObject pinkGhost;
    public GameObject orangeGhost;


    public bool hadDeathOnThisLevel = false;
    public int totalPellets;
    public int pelletsLeft;
    public int pelletCollectedInThisLife;

    public EnemyController redGhostController;
    public EnemyController blueGhostController;
    public EnemyController pinkGhostController;
    public EnemyController orangeGhostController;

    public int lives;
    public int currentLevel;

    public AudioSource startGameAudio;

    public bool gameIsRunning;

    public List<NodeController> nodeControllers = new List<NodeController>();

    public enum GhostMode
    {
        chase,
        scatter
    }

    public GhostMode currentGhostMode;


    // Start is called before the first frame update
    void Awake()
    {
        blackBackground.enabled = false;
        newGame = true;
        clearedLevel = false;
        redGhostController = redGhost.GetComponent<EnemyController>();
        pinkGhostController = pinkGhost.GetComponent<EnemyController>();
        blueGhostController = blueGhost.GetComponent<EnemyController>();
        orangeGhostController = orangeGhost.GetComponent<EnemyController>();

        ghostNodeStart.GetComponent<NodeController>().isGhostStartingNode = true;
        pacman = GameObject.Find("Player");

    }
    private void Start()
    {
        StartCoroutine(Setup());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator CollectedPellet(NodeController nodeController)
    {
        if(currentMunch == 0)
        {
            munch1.Play();
            currentMunch = 1;
        }
        else if(currentMunch == 1)
        {
            munch2.Play();
            currentMunch = 0;
        }
        pelletsLeft--;
        pelletCollectedInThisLife++;

        int requiredBluePellets = 0;
        int requiredOrangePellets = 0;

        if (hadDeathOnThisLevel)
        {
            requiredBluePellets = 12;
            requiredOrangePellets = 32;
        }
        else
        {
            requiredBluePellets = 30;
            requiredOrangePellets = 60;
        }

        if(pelletCollectedInThisLife >= requiredBluePellets && !blueGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            blueGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        if (pelletCollectedInThisLife >= requiredOrangePellets && !orangeGhost.GetComponent<EnemyController>().leftHomeBefore)
        {
            orangeGhost.GetComponent<EnemyController>().readyToLeaveHome = true;
        }

        AddToScore(10);

        if(pelletsLeft == 0)
        {
            currentLevel++;
            clearedLevel = true;
            StopGame();
            yield return new WaitForSeconds(1);
            StartCoroutine(Setup());
        }
    }

    public void AddToScore(int amount)
    {
        score += amount;
        scoreText.text = score.ToString();
    }

    public void GotPelletFromNodeController(NodeController nodeController)
    {
        nodeControllers.Add(nodeController);
        totalPellets++;
        pelletsLeft++;
    }

    public IEnumerator Setup()
    {
        //if pacman clears a level, a background will appear covering the level, and the game will pause for 0.1 seconds
        if (clearedLevel)
        {
            blackBackground.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }

        blackBackground.enabled = false;

        pelletCollectedInThisLife = 0;
        currentGhostMode = GhostMode.scatter;
        currentMunch = 0;
        gameIsRunning = false;

        float waitTimer = 1f;

        if(clearedLevel || newGame)
        {
            waitTimer = 4f;
            //Pellet will respawn
            pelletsLeft = totalPellets;
            for (int i = 0; i < nodeControllers.Count; i++)
            {
                nodeControllers[i].RespawnPellet();
            }
        }

        if (newGame)
        {
            startGameAudio.Play();
            score = 0;
            scoreText.text = 0.ToString();
            lives = 3;
            currentLevel = 1;
        }

        //pac man will setup
        pacman.GetComponent<PlayerController>().Setup();

        //ghosts will setup
        redGhostController.Setup();
        pinkGhostController.Setup();
        blueGhostController.Setup();
        orangeGhostController.Setup();

        newGame = false;
        clearedLevel = false;

        yield return new WaitForSeconds(waitTimer);

        StartGame();
    }

    void StartGame()
    {
        gameIsRunning = true;
        siren.Play();
    }

    void StopGame()
    {
        gameIsRunning = false;
        pacman.GetComponent<PlayerController>().Stop();
        siren.Stop();
    }
}
