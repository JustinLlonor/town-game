using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UISplatter : MonoBehaviour
{
    public float velocity;
    public float deceleration;
    public float shrinkSpeed;
    public Vector3 direction;

    private void Update()
    {
        velocity -= Time.deltaTime * deceleration;
        transform.position += direction * velocity * Time.deltaTime;
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
        if (transform.localScale.x < 0f) Destroy(gameObject);
    }
}
