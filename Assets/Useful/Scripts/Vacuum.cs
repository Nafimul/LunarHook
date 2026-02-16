using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Vacuum : MonoBehaviour
{
    public float suckAngle;
    public float power;
    public float volumeFadeSpeed;
    public AudioSource backgroundMusic;
    public AudioSource vacuumSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Activate());
        }
    }

    IEnumerator Activate()
    {
        vacuumSound.volume = 0;
        vacuumSound.Play();

        Vector2 suckDir = new(Mathf.Cos(suckAngle * Mathf.Deg2Rad), Mathf.Sin(suckAngle * Mathf.Deg2Rad));

        while (true)
        {
            if (backgroundMusic.volume > 0)
                backgroundMusic.volume -= volumeFadeSpeed;
            if (vacuumSound.volume < 1)
                vacuumSound.volume += volumeFadeSpeed;

            GameObject.FindWithTag("Player").GetComponent<Rigidbody2D>().AddForce(suckDir * power);
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Deactivate();
        }
    }

    void Deactivate()
    {
        StopCoroutine(Activate());
        vacuumSound.Stop();
        backgroundMusic.volume = 1.0f;
    }
}
