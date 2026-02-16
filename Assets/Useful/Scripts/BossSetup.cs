using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossSetup : MonoBehaviour
{
    public float blackScreenFadeOutDur;
    public float timeBeforeBossTalks;

    public Image blackScreen;
    public AudioSource fallingSFX;
    public AudioSource bossMusic;
    public AudioSource groundHit;
    public AudioSource moonCrashSFX;
    public State state;
    public FinalBoss boss;

    Player playerScript;
    GrapplingHook grappleScript;

    private void Awake()
    {
        state.hasDied = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerScript = GameObject.FindWithTag("Player").GetComponent<Player>();
        grappleScript = GameObject.FindWithTag("GrapplingHook").GetComponent<GrapplingHook>();
        StartCoroutine(SetUp());
    }

    IEnumerator SetUp()
    {
        playerScript.DisableAllInputExceptLooking();

        //play falling sounds
        fallingSFX.Play();
        yield return new WaitForSeconds(fallingSFX.clip.length);
        groundHit.Play();
        yield return new WaitForSeconds(groundHit.clip.length);

        //fade out the black screen
        blackScreen.CrossFadeAlpha(0, blackScreenFadeOutDur, true);
        yield return new WaitForSeconds(blackScreenFadeOutDur);
        blackScreen.enabled = false;

        //start boss
        bossMusic.Play();
        yield return new WaitForSeconds(timeBeforeBossTalks);
        StartCoroutine(boss.Talk());
        yield return new WaitUntil(() => boss.IsTalking);
        yield return new WaitUntil(() => !boss.IsTalking);

        playerScript.EnableMoonShot();
        yield return new WaitUntil(() => grappleScript.FinishedMoonPull);

        //destroy the world
        blackScreen.enabled = true;
        blackScreen.CrossFadeAlpha(255, 0, true);
        moonCrashSFX.Play();

        yield return new WaitForSeconds(moonCrashSFX.clip.length);
        SceneManager.LoadScene("Credits");
    }
}
