using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsEndDetector : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("CreditsText"))
            SceneManager.LoadScene("Menu");
    }
}
