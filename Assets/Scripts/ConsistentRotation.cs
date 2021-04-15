using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsistentRotation : MonoBehaviour
{
    [SerializeField]
    private Vector3 rotationsPerSecond;

    private void Update()
    {
        transform.Rotate(rotationsPerSecond * Time.deltaTime);
    }
}
