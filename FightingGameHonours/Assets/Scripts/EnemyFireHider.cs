using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyFireHider : MonoBehaviour
{
    public Image fire;
    private EnemyMovement enemyMovement;
    void Start()
    {
        // Assuming the TextMeshPro component is already assigned in the inspector
        fire = GetComponent<Image>();

        // Find the PlayerMovement component in the scene
        enemyMovement = FindObjectOfType<EnemyMovement>();

        // Initially hide the text
        fire.enabled = true;
    }

    void Update()
    {
        // Check if the condition is met
        if (enemyMovement.hasBalled == true)
        {
            // If condition is met, make the text visible
            fire.enabled = false;

        }
        else if (enemyMovement.hasBalled == false)
        {
            // If condition is not met, keep the text invisible
            fire.enabled = true;

        }
    }
}
