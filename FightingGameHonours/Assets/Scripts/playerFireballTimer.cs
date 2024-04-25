using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class playerFireballTimer : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private PlayerMovement playerMovement;

    private Coroutine playerFireCountdown;
    private float countdownTimer = 10f;
    void Start()
    {
        // Assuming the TextMeshPro component is already assigned in the inspector
        textMeshPro = GetComponent<TextMeshProUGUI>();

        // Find the PlayerMovement component in the scene
        playerMovement = FindObjectOfType<PlayerMovement>();

        // Initially hide the text
        textMeshPro.enabled = false;
    }

    void Update()
    {
        // Check if the condition is met
        if (playerMovement.hasBalled == true)
        {
            if (playerFireCountdown == null)
            {
                playerFireCountdown = StartCoroutine(PlayerFireCountdown());
            }

        }
        else if (playerMovement.hasBalled == false)
        {
            // Stop the countdown coroutine if it's running
            if (playerFireCountdown != null)
            {
                StopCoroutine(playerFireCountdown);
                playerFireCountdown = null;
            }
            // Reset the countdown timer
            countdownTimer = 10f;

            textMeshPro.enabled = false;
            
        }
    }

    IEnumerator PlayerFireCountdown()
    {
        // Show the text
        textMeshPro.enabled = true;

        while (countdownTimer > 0)
        {
            // Update the countdown text
            textMeshPro.text = Mathf.CeilToInt(countdownTimer).ToString();

            // Wait for one second
            yield return new WaitForSeconds(1f);

            // Decrement the countdown timer
            countdownTimer -= 1f;
        }

        // Reset the countdown timer
        countdownTimer = 10f;

        // Hide the text
        textMeshPro.enabled = false;

        // Reset hasBalled
        playerMovement.hasBalled = false;

        // Reset the coroutine reference
        playerFireCountdown = null;
    }
}