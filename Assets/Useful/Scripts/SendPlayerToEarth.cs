using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SendPlayerToEarth : MonoBehaviour
{
    public AudioSource zoomingSFX;
    public float sendSpeed;
    public float flyingDuration;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            StartCoroutine(SendToEarth());
        }
    }

    IEnumerator SendToEarth()
    {
        Vector2 EARTH_DIRECTION = new(Mathf.Cos(Mathf.Deg2Rad * 75), Mathf.Sin(Mathf.Deg2Rad * 75));
        zoomingSFX.Play();
        GameObject.FindWithTag("GrapplingHook").GetComponent<GrapplingHook>().StopShot();

        for (int i = 0; i < flyingDuration; i++)
        {
            GameObject.FindWithTag("Player").transform.position += (Vector3) EARTH_DIRECTION * sendSpeed;
            yield return new WaitForFixedUpdate();
        }

        SceneManager.LoadScene("FinalBoss");
    }
}
