using UnityEngine;

public class HeartUI : MonoBehaviour
{
    public Player playerScript;
    public GameObject heart;
    public float spaceBetweenHearts;
    GameObject[] hearts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hearts = new GameObject[playerScript.defaultHealth];
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i] = Instantiate(heart, transform.position + Vector3.right * (i * spaceBetweenHearts), transform.rotation, transform);
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < playerScript.Health; i++)
        {
            hearts[i].SetActive(true);
        }
        for (int i = playerScript.Health; i < hearts.Length; i++)
        {
            hearts[i].SetActive(false);
        }
    }
}
