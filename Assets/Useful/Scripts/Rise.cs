using UnityEngine;

public class Rise : MonoBehaviour
{
    public float speed;

    // Update is called once per frame
    void Update()
    {
        transform.position += Time.deltaTime * speed * Vector3.up;
    }
}
