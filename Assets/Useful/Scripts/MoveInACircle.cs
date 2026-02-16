using Unity.Cinemachine;
using UnityEngine;

public class MoveInACircle : MonoBehaviour
{
    public Vector2 startingCenterDistance;
    public float speed;
    Vector2 circleCenter;
    Quaternion defaultRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        circleCenter = (Vector2) transform.position + startingCenterDistance;
        defaultRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(circleCenter, new Vector3(0, 0, 1) , speed * Time.deltaTime);
        transform.rotation = defaultRotation;
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
