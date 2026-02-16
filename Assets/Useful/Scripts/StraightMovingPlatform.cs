using UnityEngine;

public class StraightMovingPlatform : MonoBehaviour
{
    public Vector2 dir;
    public float speed;
    public float timeToChangeDir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(ChangeDir), timeToChangeDir, timeToChangeDir);
        dir = dir.normalized;
    }

    void ChangeDir()
    {
        dir = -dir;
    }

    private void FixedUpdate()
    {
        transform.position += speed * Time.deltaTime * (Vector3)dir;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
}
