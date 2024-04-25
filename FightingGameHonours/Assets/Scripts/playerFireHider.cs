using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerFireHider : MonoBehaviour
{
    public Image fire;
    private PlayerMovement playerMovement;
    void Start()
    {
        // Assuming the TextMeshPro component is already assigned in the inspector
        fire = GetComponent<Image>();

        // Find the PlayerMovement component in the scene
        playerMovement = FindObjectOfType<PlayerMovement>();

        // Initially hide the text
        fire.enabled = true;
    }

    void Update()
    {
        // Check if the condition is met
        if (playerMovement.hasBalled == true)
        {
            // If condition is met, make the text visible
            fire.enabled = false;

        }
        else if (playerMovement.hasBalled == false)
        {
            // If condition is not met, keep the text invisible
            fire.enabled = true;

        }
    }
}
