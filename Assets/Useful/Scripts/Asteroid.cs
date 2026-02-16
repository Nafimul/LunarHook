using UnityEngine;

public class Asteroid : MonoBehaviour
{
    Rigidbody2D rb;
    public float rotationSpeed;
    public float rotationDirChangeRate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = rotationSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.value < rotationDirChangeRate)
        {
            rb.angularVelocity = -rb.angularVelocity;
        }
    }
}
