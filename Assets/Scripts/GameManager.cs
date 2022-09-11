using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public AudioSource powerPelletAudio;
    public AudioSource munch1;
    public AudioSource munch2;

    public AudioSource ghostEatenAudio;

    public AudioSource respawningAudio;
    public int currentMunch;

    public int score;
    public Text scoreText;

    public Text gameOverText;
    public Text livesText;

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
    public AudioSource deathAudio;

    public bool gameIsRunning;

    public int[] ghostModeTimers = new int[] { 7, 20, 7, 20, 5, 20, 5 };
    public int ghostModeTimerIndex;

    public float ghostModeTimer = 0;

    public bool runningTimer;
    public bool completedTimer;

    public bool isPowerPelletRunning = false;
    public float currentPowerPelletTime = 0;

    public float powerPelletTimer = 8f;
    public int powerPelletMultiplier = 1;


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
        if (!gameIsRunning)
        {
            return;
        }

        if(redGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning 
            || pinkGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning
            || blueGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning
            || orangeGhostController.ghostNodeState == EnemyController.GhostNodeStateEnum.respawning
            )
        {
            if (!respawningAudio.isPlaying)
            {
                respawningAudio.Play();
            }
        }
        else
        {
            if (respawningAudio.isPlaying)
            {
                respawningAudio.Stop();
            }
        }


        if(!completedTimer && runningTimer)
        {
            ghostModeTimer += Time.deltaTime;
            if(ghostModeTimer >= ghostModeTimers[ghostModeTimerIndex])
            {
                ghostModeTimer = 0;
                ghostModeTimerIndex++;
                if(currentGhostMode == GhostMode.chase)
                {
                    currentGhostMode = GhostMode.scatter;
                }
                else
                {
                    currentGhostMode = GhostMode.chase;
                }

                if(ghostModeTimerIndex == ghostModeTimers.Length)
                {
                    completedTimer = true;
                    runningTimer = false;
                    currentGhostMode = GhostMode.chase;
                }
            }
        }

        if (isPowerPelletRunning)
        {
            currentPowerPelletTime += Time.deltaTime;
            if(currentPowerPelletTime >= powerPelletTimer)
            {
                isPowerPelletRunning = false;
                currentPowerPelletTime = 0;
                powerPelletAudio.Stop();
                siren.Play();
                powerPelletMultiplier = 1;
            }
        }
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

        if (nodeController.isPowerPellet)
        {
            siren.Stop();
            powerPelletAudio.Play();
            isPowerPelletRunning = true;
            currentPowerPelletTime = 0;

            redGhostController.SetFrightened(true);
            blueGhostController.SetFrightened(true);
            pinkGhostController.SetFrightened(true);
            orangeGhostController.SetFrightened(true);
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
        ghostModeTimerIndex = 0;
        ghostModeTimer = 0;
        completedTimer = false;
        runningTimer = true;
        gameOverText.enabled = false;
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
            SetLives(3);
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
        powerPelletAudio.Stop();
        respawningAudio.Stop();
    }

    void SetLives(int newLives)
    {
        lives = newLives;
        livesText.text = lives.ToString();
    }

    public IEnumerator PauseGame(float timeToPause)
    {
        gameIsRunning = false;
        yield return new WaitForSeconds(timeToPause);
        gameIsRunning = true;
    }

    public void GhostEaten()
    {
        StartCoroutine(PauseGame(1));
        ghostEatenAudio.Play();
        AddToScore(400 * powerPelletMultiplier);
        powerPelletMultiplier++;
    }
    public IEnumerator PlayerEaten()
    {
        hadDeathOnThisLevel = true;
        StopGame();
        yield return new WaitForSeconds(1);

        redGhostController.SetVisible(false);
        orangeGhostController.SetVisible(false);
        pinkGhostController.SetVisible(false);
        blueGhostController.SetVisible(false);

        pacman.GetComponent<PlayerController>().Death();
        deathAudio.Play();

        yield return new WaitForSeconds(3);

        SetLives(lives - 1);
        if(lives <= 0)
        {
            newGame = true;
            //Display gameover text
            gameOverText.enabled = true;
            yield return new WaitForSeconds(3);
            SceneManager.LoadScene("GameOverScene");
            yield return new WaitForSeconds(3);
        }

        StartCoroutine(Setup());
    }

}
