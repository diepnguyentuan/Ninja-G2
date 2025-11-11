using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform player;

    void LateUpdate()
    {
        Vector3 newPos = player.position;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }
}
