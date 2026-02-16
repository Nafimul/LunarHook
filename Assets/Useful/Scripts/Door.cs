using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    int enemiesLeftToKill;
    public bool OpenAtStart;
    public AudioSource openSFX;
    public AudioSource closeSFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (OpenAtStart)
        {
            Open(false);
        }
    }

    public void RemoveEnemyToKill()
    {
        enemiesLeftToKill--;
        if (enemiesLeftToKill <= 0 )
        {
            Open(true);
        }
    }

    public void AddEnemyToKill()
    {
        enemiesLeftToKill++;
    }

    void Open(bool shouldPlaySound)
    {
        if (shouldPlaySound)
            openSFX.Play();
        GetComponent<Animator>().SetTrigger("Open");
        GetComponent<BoxCollider2D>().enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    void Close(bool shouldPlaySound)
    {
        if (shouldPlaySound)
            closeSFX.Play();
        GetComponent<Animator>().SetTrigger("Close");
        GetComponent<BoxCollider2D>().enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Obstacle");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && OpenAtStart)
        {
            Close(true);
            OpenAtStart = false;
        }
    }
}
