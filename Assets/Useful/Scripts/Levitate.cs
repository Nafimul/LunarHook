using UnityEngine;

public class Levitate : MonoBehaviour
{
    public float speed;
    public float height;
    float i;

    // Update is called once per frame
    void Update()
    {
        transform.position += height * Mathf.Sin(i) * Vector3.up;
        i += speed;
        if (i > 1000)
            i -= 200 * Mathf.PI;
    }
}
