using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class WideCameraArea : MonoBehaviour
{
    public CinemachineCamera cam;
    public float targetOrthographicSize;
    public float zoomOutSpeed;

    float startOrthographicSize;

    private void Start()
    {
        startOrthographicSize = cam.Lens.OrthographicSize;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(ZoomCam(targetOrthographicSize));
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") 
            //to get rid of errors when the scene is reloaded
            && gameObject.activeInHierarchy)
        {
            StartCoroutine(ZoomCam(startOrthographicSize));
        }
    }

    IEnumerator ZoomCam(float to)
    {
        while (cam.Lens.OrthographicSize < targetOrthographicSize)
        {
            cam.Lens.OrthographicSize = (Mathf.Lerp(cam.Lens.OrthographicSize, to, Time.deltaTime * zoomOutSpeed));
            yield return null;
        }
    }
}
