using System.Collections;
using System.ComponentModel.Design;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float speed;
    public float pointerDist;
    public int defaultHealth;
    public float invincibilityFramesDuration;
    public float flashesPerSecWhileInvincible;
    public float killSpeedBoostPower;
    public int collisionDamage;

    public bool invincibleForTesting;

    Head grappleHeadScript;
    bool invincible;
    public int Health { get; private set; }

    public GrapplingHook grappleScript;
    public HeartUI heartUI;
    public TMPro.TMP_Text coinCountUI;
    public AudioSource coinSFX;
    public AudioSource deathSFX;
    public AudioSource damagedSFX;
    public AudioSource backgroundMusic;
    public GameObject gameOverScreen;
    public GameObject controllerPointer;
    public State state;
    public Buttons buttonScript; 

    InputAction retractionShot;
    InputAction moonShot;
    InputAction swingShot;
    InputAction MouseLook;
    InputAction GamepadLook;
    InputAction cancelShot;
    InputAction pause;
    //InputAction move;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    public Vector2 WhereToLook { get; set; }

    private void Awake()
    {
        state.playerControls = new InputSystem_Actions();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = rb.GetComponent<SpriteRenderer>();
        grappleHeadScript = GameObject.FindWithTag("GrappleHead").GetComponent<Head>();

        Health = defaultHealth;
        coinCountUI.text = "" + state.coinNumsCollected.Count;

        if (state.hasDied)
            Respawn();
        else
            state.spawnpoint = transform.position;

        controllerPointer.SetActive(false);
    }
    void Respawn()
    {
        transform.position = state.spawnpoint;
        Time.timeScale = 1;
    }

    private void OnEnable()
    {
        moonShot = state.playerControls.Player.Shoot1;

        retractionShot = state.playerControls.Player.Shoot1;
        retractionShot.Enable();
        retractionShot.performed += RetractionShot;
        retractionShot.canceled += EndRetraction;

        swingShot = state.playerControls.Player.Shoot2;
        swingShot.Enable();
        swingShot.performed += SwingShot;
        swingShot.canceled += EndSwing;

        cancelShot = state.playerControls.Player.CancelShot;
        cancelShot.Enable();
        cancelShot.performed += CancelShot;

        //move = state.playerControls.Player.Move;
        //move.Enable();
        //move.performed += Move;

        MouseLook = state.playerControls.Player.MouseLook;
        MouseLook.Enable();
        MouseLook.performed += LookPos;

        GamepadLook = state.playerControls.Player.GamepadLook;
        GamepadLook.Enable();
        GamepadLook.performed += LookDir;

        pause = state.playerControls.Player.Pause;
        pause.Enable();
        pause.performed += Pause;
    }

    private void OnDisable()
    {
        retractionShot.Disable();
        swingShot.Disable();
        cancelShot.Disable();
        MouseLook.Disable();
        GamepadLook.Disable();
        moonShot.Disable();
        pause.Disable();
        //move.Disable();

        state.playerControls.Player.Disable();
    }

    void RetractionShot(InputAction.CallbackContext context)
    {
        if (!MouseClickingUI())
        {
            grappleScript.StopShot();
            StartCoroutine(grappleScript.RetractionShot());
        }
    }

    void Pause(InputAction.CallbackContext context)
    {
        buttonScript.Pause();
    }

    bool MouseClickingUI()
    {
        return EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButton(0);
    }

    //void Move(InputAction.CallbackContext context)
    //{
    //    Debug.Log("moving");
    //}

    void SwingShot(InputAction.CallbackContext context)
    {
        if (!MouseClickingUI())
        {
            grappleScript.StopShot();
            StartCoroutine(grappleScript.SwingShot());
        }
    }

    void EndSwing(InputAction.CallbackContext context)
    {
        grappleScript.ShouldEndSwing = true;
    }

    void EndRetraction(InputAction.CallbackContext context)
    {
        grappleScript.ShouldEndRetraction = true;
    }

    void CancelShot(InputAction.CallbackContext context)
    {
        grappleScript.StopShot();
    }

    public void Look()
    {
        if (!grappleScript.IsGrappling || grappleHeadScript.IsWieldingObj)
        {
            grappleScript.PointTowards(WhereToLook);
        }
    }

    void LookPos(InputAction.CallbackContext context)
    {
        WhereToLook = Camera.main.ScreenToWorldPoint(context.ReadValue<Vector2>());
        controllerPointer.SetActive(false);
        Cursor.visible = true;
        Look();
    }

    void LookDir(InputAction.CallbackContext context)
    {
        controllerPointer.SetActive(true);
        Cursor.visible = false;
        controllerPointer.transform.localPosition = context.ReadValue<Vector2>() * pointerDist;
        WhereToLook = controllerPointer.transform.position;
        Look();
    }

    void MoonShot(InputAction.CallbackContext context)
    {
        if (!MouseClickingUI())
        {
            grappleScript.StopShot();
            StartCoroutine(grappleScript.MoonShot());
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("InstantKill"))
            StartCoroutine(Die());
        else if (collider.gameObject.CompareTag("Damaging"))
            GetDamaged();
        else if (collider.gameObject.CompareTag("Coin"))
        {
            CollectCoint(collider.gameObject);
        }
        else if (collider.gameObject.CompareTag("Checkpoint"))
        {
            SetCheckpoint(collider.gameObject);
        }
    }

    void SetCheckpoint(GameObject checkpoint)
    {
        //if it's a new checkpoint
        if (checkpoint.GetComponent<Checkpoint>().num > state.lastCheckpointNum)
        {
            state.secondToLastCheckpoint = state.lastCheckpoint;
            state.secondToLastCheckpointNum = state.lastCheckpointNum;

            state.lastCheckpoint = checkpoint.transform.position;
            state.lastCheckpointNum = checkpoint.GetComponent<Checkpoint>().num;

            //set the spawnpoint to the second to last checkpoint reached
            if (state.secondToLastCheckpoint != Vector2.zero)
                state.spawnpoint = state.secondToLastCheckpoint;

            //max out health
            Health = defaultHealth;
            heartUI.Refresh();
        }
    }

    void CollectCoint(GameObject coin)
    {
        coinSFX.Play();
        state.coinNumsCollected.Add(coin.GetComponent<Coin>().number);
        coinCountUI.text = "" + state.coinNumsCollected.Count;
        coin.SetActive(false);
    }

    IEnumerator Die()
    {
        if (!invincible)
        {
            state.hasDied = true;
            deathSFX.Play();
            backgroundMusic.Stop();
            grappleScript.StopShot();
            rb.linearVelocity = Vector3.zero;
            gameOverScreen.SetActive(true);
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(deathSFX.clip.length);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void GetDamaged()
    {
        if (!invincible && !invincibleForTesting)
        {
            Health--;
            if (Health <= 0)
                StartCoroutine(Die());
            else
            {
                heartUI.Refresh();
                damagedSFX.Play();
                StartCoroutine(InvincibilityFrames());
            }
        }
    }

    IEnumerator InvincibilityFrames()
    {
        invincible = true;

        for (float j = 0; j < invincibilityFramesDuration; j += (1 / flashesPerSecWhileInvincible))
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds((1 / flashesPerSecWhileInvincible) / 2);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds((1 / flashesPerSecWhileInvincible) / 2);
        }

        invincible = false;
    }

    public void KillSpeedBoost(Vector2 towards)
    {
        grappleScript.StopShot();
        rb.linearVelocity = killSpeedBoostPower * (towards - (Vector2)transform.position).normalized;
    }

    public void DisableAllInputExceptLooking()
    {
        retractionShot.Disable();
        swingShot.Disable();
        cancelShot.Disable();
        moonShot.Disable();
    }

    public void EnableMoonShot()
    {
        moonShot.Enable();
        moonShot.performed += MoonShot;
        moonShot.performed -= RetractionShot;
    }

    public void DisableMoonShot()
    {
        moonShot.Disable();
    }
}
