using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    //Set up character
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private float dirX = 0f;
    [SerializeField] private float moveSpeed = 4f;

    //Animations
    public bool isPunching = false;
    public bool isCrouching = false;
    public bool isBlocking = false;
    public bool isBalling = false;
    public bool isKicking = false;

    //Fireabll cooldown conditoon
    public bool hasBalled = false;

    //Sound effects
    [SerializeField] private AudioSource regBlockSoundEffect;
    [SerializeField] private AudioSource fireballBlockSoundEffect;
    [SerializeField] private AudioSource regAttackSoundEffect;
    [SerializeField] private AudioSource fireBallAttackSoundEffect;
    [SerializeField] private AudioSource fireBallSizzleSoundEffect;
    [SerializeField] private AudioSource blueWin;
    [SerializeField] private AudioSource punchSoundEffect;
    [SerializeField] private AudioSource fireballSoundEffect;
    [SerializeField] private AudioSource kickSoundEffect;
    //Health Logic
    public int maxHealth = 100;
    public int currentHealth;
    public EnemyHealthBar healthbar;

    //Block Stamina Logic
    public int maxStamina = 100;
    public float currentStamina;
    public EnemyBlockBar staminabar;
    private bool attackedStam = false;
    private Coroutine rechargeStaminaCoroutine;
    private float lastHitTime;

    //MlAgent script
    public EnemyAgent enemyAgent;
    public EnemyAgentImpossible enemyAImp;


    //Defeated Text/Player Winner Text
    public TextMeshProUGUI BLUEWINTEXT;
    public GameObject enemyBlockBar;
    public GameObject playerBlockBar;
    public GameObject enemyHealthBar;
    public GameObject playerHealthBar;
    public Image playerHead;
    public Image enemyHead;
    public Image PlayerFire;
    public Image EnemyFire;
    public TextMeshProUGUI PlayerFireTimer;
    public TextMeshProUGUI EnemyFireTimer;
    public AudioSource BgMusic;
    public TextMeshProUGUI Difficulty;

    //Player attacks
    //Fireball
    public EnemyProjectileBehaviour ProjectilePrefab;
    public Transform LaunchOffset;
    //Punch
    public Transform punchPoint;
    public float punchRange = 0.1f;
    //Kick
    public Transform kickPoint;
    public float kickRange = 0.1f;
    public LayerMask playerLayer;

    //Wins of the player
    public static int wins = 0;


    // Start is called before the first frame update
    void Start()
    {

        // Load the value of wins from PlayerPrefs
        wins = PlayerPrefs.GetInt("Wins", 0);

        //set up
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        //Animations
        anim = GetComponent<Animator>();

        //Health
        currentHealth = maxHealth;
        healthbar.SetMaxHealth(maxHealth);

        //Blocking
        currentStamina = maxStamina;
        staminabar.SetMaxStamina(maxStamina);

        // Start coroutine to continuously recharge stamina
        rechargeStaminaCoroutine = StartCoroutine(RechargeStamina());
    }

    private void Update()
    {

        // If currently punching, crouching, blocking, or performing other actions, disable movement
        if (isPunching || isCrouching || isBlocking || isKicking || isBalling)
        {
            dirX = 0f; // Stop movement if any action is being performed
        }
        else
        {
            // Allow Movement only with left and right arrow keys
            float rawInput = 0f;
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                rawInput -= 1f;
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                rawInput += 1f;
            }
            dirX = rawInput;
        }

        // Apply horizontal movement to the Rigidbody
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y); 
        UpdateWalkAnimationState();

        // Block on "L" key down
        if (Input.GetKeyDown(KeyCode.L) && !isCrouching && !isPunching && !isKicking && !isBalling)
        {
            Block();
        }

        // Release block on "L" key up or if stamina is depleted
        else if (Input.GetKeyUp(KeyCode.L) || currentStamina == 0)
        {
            ReleaseBlock();
        }


        // Crouch on "DownArrow" key down
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isBlocking && !isPunching && !isKicking && !isBalling)
        {
            Crouch();
        }
        // Stand up on "DownArrow" key up
        else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            StandUp();
        }

        // Punch on "N" key down
        if (Input.GetKeyDown(KeyCode.N) && !isCrouching && !isBlocking && !isPunching && !isKicking && !isBalling)
        {
            Punch();
        }


        //Kick on " M" key down
        if (Input.GetKeyDown(KeyCode.M) && !isCrouching && !isBlocking && !isPunching && !isKicking && !isBalling)
        {
            Kick();
        }

        //Fireball on "Space" key down
        if (Input.GetKeyDown(KeyCode.RightShift) && !isCrouching && !isBlocking && !isPunching && !isBalling && !isKicking)
        {
            StartCoroutine(Fireball());
        }
    }

    //Punch Animation
    public void Punch()
    {
        if (!isCrouching && !isBlocking && !isKicking && !isBalling)
        {
            isPunching = true;
            punchSoundEffect.Play();
            anim.SetTrigger("punch");

            //Setting up attack collision detection
            Collider2D[] punchEnemies = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, playerLayer);
            foreach (Collider2D player in punchEnemies)
            {
                Debug.Log("Punched");
                player.GetComponent<PlayerMovement>().Punched(10);
            }
        }
    }

    //Fireball Animation
    public IEnumerator Fireball()
    {
        if (!isCrouching && !isBlocking && !isPunching && !isKicking && !hasBalled)
        {
            isBalling = true;
            hasBalled = true;
            fireballSoundEffect.Play();
            anim.SetTrigger("fireball");
            Instantiate(ProjectilePrefab, LaunchOffset.position, LaunchOffset.rotation);
            yield return new WaitForSeconds(10f);
            hasBalled = false;
        }
    }

    //Kick Animation
    public void Kick()
    {
        if (!isCrouching && !isBlocking && !isPunching && !isBalling)
        {
            isKicking = true;
            kickSoundEffect.Play();
            anim.SetTrigger("kick");

            //Setting up attack collision detection
            Collider2D[] kickEnemies = Physics2D.OverlapCircleAll(kickPoint.position, kickRange, playerLayer);
            foreach (Collider2D player in kickEnemies)
            {
                Debug.Log("Kicked");
                player.GetComponent<PlayerMovement>().Kicked(15);
            }
        }
    }

    // Called from animation event to signal the end of punch animation
    public void FinishPunch()
    {
        isPunching = false;
    }

    // Called from animation event to signal the end of fireball animation
    public void FinishFireBall()
    {
        isBalling = false;
    }

    // Called from animation event to signal the end of kick animation
    public void FinishKick()
    {
        isKicking = false;
    }

    // Updates animations for player movement
    private void UpdateWalkAnimationState()
    {
        if (dirX != 0f)
        {
            anim.SetBool("walking", true);
        }
        else
        {
            anim.SetBool("walking", false);
        }
    }

    //Crouch Animation
    public void Crouch()
    {
        if (!isBlocking)
        {
            isCrouching = true;
            anim.SetBool("crouch", true);
        }
    }

    //Finish Crouch
    public void StandUp()
    {
        isCrouching = false;
        anim.SetBool("crouch", false);
    }

    //Enemy Block
    public void Block()
    {
        if (currentStamina > 0 && !isCrouching)
        {
            isBlocking = true;
            anim.SetBool("block", true);
        }
    }

    //Block Finished
    public void ReleaseBlock()
    {
        isBlocking = false;
        anim.SetBool("block", false);
    }

    // Coroutine to continuously recharge stamina
    private IEnumerator RechargeStamina()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.001f); // Wait for 1 millisecond before recharging

            if (Time.time - lastHitTime >= 5f && currentStamina < maxStamina)
            {
                currentStamina += 0.05f; // Recharge 0.05 points per millisecond
                staminabar.SetStamina((int)currentStamina);
            }
        }
    }

    //Take damage on punch
    public void Punched(int damage)
    {
        if (isBlocking)
        {
            if (currentStamina >= damage)
            {
                currentStamina -= damage;
                //Update ML agent reward
                //enemyAgent.beenPunchedStam();
                enemyAImp.beenPunchedStam();
            }
            else
            {
                currentStamina = 0;
            }
            regBlockSoundEffect.Play();
            staminabar.SetStamina((int)currentStamina);
            attackedStam = true;
            lastHitTime = Time.time; // Update the last hit time

            // If coroutine is running, reset it
            if (rechargeStaminaCoroutine != null)
            {
                StopCoroutine(rechargeStaminaCoroutine);
                rechargeStaminaCoroutine = StartCoroutine(RechargeStamina());
            }

            // Apply knockback force to the right
            GetComponent<Rigidbody2D>().AddForce(Vector2.right * 2f, ForceMode2D.Impulse);
        }
        else
        {
            if(currentHealth >= damage)
            {
                currentHealth -= damage;
                regAttackSoundEffect.Play();
                healthbar.SetHealth(currentHealth);
                //Update ML agent reward
                //enemyAgent.beenPunchedHp();
                enemyAImp.beenPunchedHp();
            }
            else
            {
                //Enemy has been defeated
                blueWins();
            }
        }
    }

    //Take damage on kick
    public void Kicked(int damage)
    {
        if (isBlocking)
        {
            if (currentStamina >= damage)
            {
                currentStamina -= damage;
                //Update ML agent reward
                //enemyAgent.beenKickedStam();
                enemyAImp.beenKickedStam();
            }
            else
            {
                currentStamina = 0;
            }
            regBlockSoundEffect.Play();
            staminabar.SetStamina((int)currentStamina);
            attackedStam = true;
            lastHitTime = Time.time; // Update the last hit time

            // If coroutine is running, reset it
            if (rechargeStaminaCoroutine != null)
            {
                StopCoroutine(rechargeStaminaCoroutine);
                rechargeStaminaCoroutine = StartCoroutine(RechargeStamina());
            }

            // Apply knockback force to the right
            GetComponent<Rigidbody2D>().AddForce(Vector2.right * 2f, ForceMode2D.Impulse);
        }
        else
        {
            if (currentHealth >= damage)
            {
                currentHealth -= damage;
                regAttackSoundEffect.Play();
                healthbar.SetHealth(currentHealth);
                //Update ML agent reward
                //enemyAgent.beenKickedHp();
                enemyAImp.beenKickedHp();
            }
            else
            {
                //Enemy has been defeated
                blueWins();
            }
        }
    }

    //Take damage on balled
    public void Balled(int damage)
    {
        if (isBlocking)
        {
            if (currentStamina >= damage)
            {
                currentStamina -= damage;
                //Update ML agent reward
                //enemyAgent.beenBalledStam();
                enemyAImp.beenBalledStam();
            }
            else
            {
                currentStamina = 0;
            }
            fireballBlockSoundEffect.Play();
            fireBallSizzleSoundEffect.Play();
            staminabar.SetStamina((int)currentStamina);
            attackedStam = true;
            lastHitTime = Time.time; // Update the last hit time

            // If coroutine is running, reset it
            if (rechargeStaminaCoroutine != null)
            {
                StopCoroutine(rechargeStaminaCoroutine);
                rechargeStaminaCoroutine = StartCoroutine(RechargeStamina());
            }

            // Apply knockback force to the right
            GetComponent<Rigidbody2D>().AddForce(Vector2.right * 5f, ForceMode2D.Impulse);
        }
        else
        {
            if (currentHealth >= 30)
            {
                currentHealth -= 30;
                fireBallAttackSoundEffect.Play();
                fireBallSizzleSoundEffect.Play();
                healthbar.SetHealth(currentHealth);
                //Update ML agent reward
                //enemyAgent.beenBalledHp();
                enemyAImp.beenBalledHp();
            }
            else
            {
                //Enemy has been defeated
                blueWins();
            }
        }
    }

    public void blueWins()
    {
        //Update wins data
        wins++;
        Debug.Log("Wins after win: " + wins);
        PlayerPrefs.SetInt("Wins", wins); // Save the updated wins count
        PlayerPrefs.Save(); // Ensure the data is saved immediately


        currentHealth = 0;
        Debug.Log("Enemy defeated");
        
        //GUI 
        BLUEWINTEXT.gameObject.SetActive(true);
        enemyBlockBar.gameObject.SetActive(false);
        playerBlockBar.gameObject.SetActive(false);
        enemyHealthBar.gameObject.SetActive(false);
        playerHealthBar.gameObject.SetActive(false);
        playerHead.gameObject.SetActive(false);
        enemyHead.gameObject.SetActive(false);
        PlayerFire.gameObject.SetActive(false);
        EnemyFire.gameObject.SetActive(false);
        PlayerFireTimer.gameObject.SetActive(false);
        EnemyFireTimer.gameObject.SetActive(false);
        Difficulty.gameObject.SetActive(false);
        BgMusic.Stop();
        blueWin.Play();
        Time.timeScale = 0f;
        StartCoroutine(RestartSceneAfterDelay(2f));
    }

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        // Check if wins count has reached three
        if (wins >= 3)
        {
            wins = 0; // Reset wins count
            PlayerPrefs.SetInt("Wins", wins); // Save the updated wins count
            PlayerPrefs.Save(); // Ensure the data is saved immediately

            // Check if there's a next scene in the build settings
            if (SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings)
            {
                // Load the next scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                // Restart the current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        else if (wins <= -3) // If wins count reaches -3, load the previous scene
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        else
        {
            // Restart the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        Time.timeScale = 1f; // Ensure time scale is reset
    }
}
