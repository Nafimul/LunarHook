using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Vector3 activatedPos;
    public float maxDist;

    private void Start()
    {
        StartCoroutine(DeactivateIfTooFar());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.isTrigger) 
            Deactivate();
    }

    public void Activate()
    {
        this.gameObject.SetActive(true);
        activatedPos = transform.position;
    }

    void Deactivate()
    {
        this.gameObject.SetActive(false);
    }

    IEnumerator DeactivateIfTooFar()
    {
        yield return new WaitUntil(() => activatedPos != null);

        while (true)
        {
            yield return new WaitUntil(() => (transform.position - activatedPos).magnitude > maxDist);
            Deactivate();
        }
    }
}
