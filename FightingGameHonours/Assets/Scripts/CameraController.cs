using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public List<Transform> players;
    public Vector3 offset;
    private Vector3 velocity;
    public float smoothTime = .5f;
    private void LateUpdate()
    {
        if (players.Count == 0)
        {
            return;
        }
        
        // Calculate the midpoint between the player and the enemy
        Vector3 midPoint = GetMidPoint();
        
        Vector3 newPosition = midPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime );
    }

    Vector3 GetMidPoint()
    {
        if (players.Count == 1)
        {
            return players[0].position;
        }

        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 0; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }
        return bounds.center;
    }

}


