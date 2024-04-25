using System;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    //Set up character
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    private float dirX = 0f;
    [SerializeField] private float moveSpeed = 4f;

    //Player moves
    private bool isPunching = false;
    private bool isCrouching = false;
    private bool isBlocking = false;
    private bool isBalling = false;
    private bool isKicking = false;

    //Fireabll cooldown conditoon
    public bool hasBalled = false;
    
    //Character sound effects
    [SerializeField] private AudioSource punchSoundEffect;
    [SerializeField] private AudioSource fireballSoundEffect;
    [SerializeField] private AudioSource kickSoundEffect;
    [SerializeField] private AudioSource regBlockSoundEffect;
    [SerializeField] private AudioSource regAttackSoundEffect;
    [SerializeField] private AudioSource fireballBlockSoundEffect;
    [SerializeField] private AudioSource fireBallSizzleSoundEffect;
    [SerializeField] private AudioSource fireBallAttackSoundEffect;
    [SerializeField] private AudioSource redWin;


    //Player attacks
    //Fireball
    public PlayerProjectileBehaviour ProjectilePrefab;
    public Transform LaunchOffset;
    //Punch
    public Transform punchPoint;
    public float punchRange = 0.1f;
    //Kick
    public Transform kickPoint;
    public float kickRange = 0.1f;
    public LayerMask enemyLayer;


    //Defeated Text/Player Winner Text
    public TextMeshProUGUI REDWINTEXT;
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

    // Start is called before the first frame update
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
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

    // Update is called once per frame
    private void Update()
    {

        // If currently punching, crouching, or blocking, disable movement
        if (isPunching || isCrouching || isBlocking || isBalling || isKicking)
        {
            dirX = 0f;
        }
        else
        {
            // Allow Movement only with 'A' and 'D' keys
            float rawInput = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                rawInput -= 1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                rawInput += 1f;
            }
            dirX = rawInput;
        }

        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        UpdateAnimationState();

        // Punch on "E" key down
        if (Input.GetKeyDown(KeyCode.E) && !isCrouching && !isBlocking && !isPunching && !isBalling && !isKicking)
        {
            Punch();
        }

        //Fireball on "Space" key down
        if (Input.GetKeyDown(KeyCode.Space) && !isCrouching && !isBlocking && !isPunching && !isBalling && !isKicking)
        {
            StartCoroutine(Fireball());
        }

        //Kick on "F" key down
        if (Input.GetKeyDown(KeyCode.F) && !isCrouching && !isBlocking && !isPunching && !isBalling && !isKicking)
        {
            Kick();
        }

        // Crouch on "S" key down
        if (Input.GetKeyDown(KeyCode.S) && !isPunching && !isBlocking && !isBalling && !isKicking)
        {
            Crouch();
        }
        // Stand up on "S" key up
        else if (Input.GetKeyUp(KeyCode.S))
        {
            StandUp();
        }

        // Block on "Q" key down
        if (Input.GetKeyDown(KeyCode.Q) && !isCrouching && !isPunching && !isBalling && !isKicking)
        {
            Block();
        }
        // Release block on "Q" key up
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            ReleaseBlock();
        }
    }

    // Updates animations for player movement
    private void UpdateAnimationState()
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

    //Punch Animation
    private void Punch()
    {
        if (!isCrouching && !isBlocking && !isBalling && !isKicking)
        {
            isPunching = true;
            punchSoundEffect.Play();
            anim.SetTrigger("punch");
            
            //Setting up attack collision detection
            Collider2D[] punchEnemies = Physics2D.OverlapCircleAll(punchPoint.position, punchRange, enemyLayer);
            foreach (Collider2D enemy in punchEnemies)
            {
                Debug.Log("Punched");
                enemy.GetComponent<EnemyMovement>().Punched(10);
            }
        }
    }


    //Fireball Animation
    private IEnumerator Fireball()
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
    private void Kick()
    {
        if (!isCrouching && !isBlocking && !isPunching && !isBalling)
        {
            isKicking = true;
            kickSoundEffect.Play();
            anim.SetTrigger("kick");

            //Setting up attack collision detection
            Collider2D[] kickEnemies = Physics2D.OverlapCircleAll(kickPoint.position, kickRange, enemyLayer);
            foreach (Collider2D enemy in kickEnemies)
            {
                Debug.Log("Kicked");
                enemy.GetComponent<EnemyMovement>().Kicked(15);
            }
        }
    }

    //Crouch Animation
    private void Crouch()
    {
        if (!isPunching && !isBlocking && !isBalling && !isKicking)
        {
            isCrouching = true;
            anim.SetBool("crouch", true);
        }
    }

    //Finish Crouch
    private void StandUp()
    {
        isCrouching = false;
        anim.SetBool("crouch", false);
    }

    //Block Animation
    private void Block()
    {
        if (!isPunching && !isCrouching && !isBalling && !isKicking)
        {
            isBlocking = true;
            anim.SetBool("block", true);
        }
    }

    //Block Finished
    private void ReleaseBlock()
    {
        isBlocking = false;
        anim.SetBool("block", false);
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
                //enemyAgent.punchPlayerStam();
                enemyAImp.punchPlayerStam();
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
                //enemyAgent.punchPlayerHp();
                enemyAImp.punchPlayerHp();
            }
            else
            {
                //Enemy has been defeated
                redWins();
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
                //enemyAgent.kickPlayerStam();
                enemyAImp.kickPlayerStam();
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
                //enemyAgent.kickPlayerHp();
                enemyAImp.kickPlayerHp();
            }
            else
            {
                //Enemy has been defeated
                redWins();
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
                //enemyAgent.ballPlayerStam();
                enemyAImp.ballPlayerStam();
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
                //enemyAgent.ballPlayerHp();
                enemyAImp.ballPlayerHp();
            }
            else
            {
                //Enemy has been defeated
                redWins();
            }
        }
    }

    public void redWins()
    {
        currentHealth = 0;
        Debug.Log("Enemy defeated");

        // Decrement wins variable in EnemyMovement class
        int wins = PlayerPrefs.GetInt("Wins", 0);
        wins--;
        PlayerPrefs.SetInt("Wins", wins); // Save the updated wins count
        PlayerPrefs.Save(); // Ensure the data is saved immediately
        Debug.Log("Wins after loss: " + wins);

        //GUI 
        REDWINTEXT.gameObject.SetActive(true);
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
        redWin.Play();
        Time.timeScale = 0f;

        StartCoroutine(RestartSceneAfterDelay(2f));
    }

    private IEnumerator RestartSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        // Check if the win count reaches -3
        int wins = PlayerPrefs.GetInt("Wins", 0);
        if (wins <= -3)
        {
            // Load the previous scene
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex - 1);
        }
        else
        {
            // Restart the scene
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.buildIndex);
        }
        Time.timeScale = 1f; // Ensure time scale is reset
    }
}
