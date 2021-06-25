using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillBlades : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1.0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.fixedDeltaTime);
    }
}
