using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class EnemyAgentVEasy : Agent
{

    [SerializeField] private Transform playerTransform;
    public PlayerMovement playerMovement;
    public EnemyMovement enemyMovement;
    private Animator anim;
    private float dirX = 0f;

    bool isFireballOnCooldown = false;
    float fireballCooldownDuration = 10f;
    //Obseravtions for the enemy to collect

    void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        UpdateWalkAnimationState();
    }
    public override void OnEpisodeBegin()
    {
        transform.position = new Vector3(6.11000013f, -0.130369306f, 0f);
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //Enemy position
        sensor.AddObservation(transform.position);
        //Player position
        sensor.AddObservation(playerTransform.position);
        //Player Health
        sensor.AddObservation(playerMovement.currentHealth);
        //Enemy Health
        sensor.AddObservation(enemyMovement.currentHealth);
        //Player BlockStam
        sensor.AddObservation(playerMovement.currentStamina);
        //EnemyBlockStam
        sensor.AddObservation(enemyMovement.currentStamina);

        //Enemy Fireball recharge timer
        //Player Fireball Recharge Timer
        //Player crouching?
        //Player Blocking?
    }

    //Actions the enemy can make
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!enemyMovement.isPunching && !enemyMovement.isKicking && !enemyMovement.isBalling && !enemyMovement.isBlocking && !enemyMovement.isCrouching)
        {
            float moveX = actions.ContinuousActions[0];
            dirX = moveX;
            float moveSpeed = 4f;
            transform.position += new Vector3(moveX, 0, 0) * Time.deltaTime * moveSpeed;
            // Check if the agent's discrete action is to punch

            if (actions.DiscreteActions[0] == 0)
            {
                // If the agent chooses to punch, execute the punch action
                enemyMovement.Punch();
            }
            // Check if the agent's discrete action is to kick
            if (actions.DiscreteActions[0] == 1)
            {
                // If the agent chooses to kick, execute the kick action
                enemyMovement.Kick();
            }

            else if (actions.DiscreteActions[0] == 2)
            {
                if (!isFireballOnCooldown)
                {
                    StartCoroutine(enemyMovement.Fireball());
                    isFireballOnCooldown = true;
                    StartCoroutine(FireballCooldown());
                }
            }
        }
    }

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

    IEnumerator FireballCooldown()
    {
        yield return new WaitForSeconds(fireballCooldownDuration);
        isFireballOnCooldown = false;
    }

    //Punch reward scores
    public void punchPlayerHp()
    {
        AddReward(+3f);
    }

    public void punchPlayerStam()
    {
        AddReward(+2f);
    }

    public void beenPunchedHp()
    {
        AddReward(-3f);
    }

    public void beenPunchedStam()
    {
        AddReward(-2f);
    }


    //Kick reward scores
    public void kickPlayerHp()
    {
        AddReward(+4f);
    }

    public void kickPlayerStam()
    {
        AddReward(+3f);
    }

    public void beenKickedHp()
    {
        AddReward(-4f);
    }

    public void beenKickedStam()
    {
        AddReward(-3f);
    }


    //Fireball reward scores
    public void ballPlayerHp()
    {
        AddReward(+5f);
    }

    public void ballPlayerStam()
    {
        AddReward(+4f);
    }

    public void beenBalledHp()
    {
        AddReward(-5f);
    }

    public void beenBalledStam()
    {
        AddReward(-4f);
    }


    //Win/Loss reward scored
    public void enemyWin()
    {
        AddReward(+7f);
        EndEpisode();
    }

    public void playeryWin()
    {
        AddReward(-7f);
        EndEpisode();
    }
}
