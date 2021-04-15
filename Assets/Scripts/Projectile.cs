using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float forwardForce = 100f;

    [SerializeField]
    private float upwardForce = 10f;

    [SerializeField]
    private float lifetime = 5f;

    private Rigidbody _rigidbody;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = _transform.position;
    }

    public void Fire()
    {
        var forwardVector = _transform.forward * forwardForce;
        var upwardVector = _transform.up * upwardForce;
        _rigidbody.AddForce(forwardVector + upwardVector, ForceMode.Impulse);
        StartCoroutine(DestroyCountdown());
    }

    public void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }

    private IEnumerator DestroyCountdown()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}