using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    public float shotPower;
    public float retractionPower;
    public float headReturnSpeed;
    public float maxShotDist;
    public float maxRetractionTime;
    public float swingPropulsion;
    public float swingEndPropulsionMultiplier;
    public float closenessToHeadBeforeReconnecting;
    public float objClosenessBeforeWielding;
    public float objectDetectionDist;
    public float objShootPower;
    public float shotObjUntouchableTime;

    public float moonPullDur;
    public float moonGrowSpeed;
    public float moonPullSpeed;
    public float headShrinkSpeedOnMoonShot;
    public float camRiseSpeed;

    public bool IsGrappling { get; private set; }
    public bool IsRetractingShot { get; private set; }
    public bool IsSwinging { get; private set; }

    public bool ShouldEndRetraction { private get; set; }
    public bool ShouldEndSwing { private get; set; }
    public bool FinishedMoonPull { get; private set; }

    bool isShooting;
    bool hit;
    bool handleCanMove;

    public AudioSource shootSFX;
    public AudioSource hitSFX;
    public AudioSource zipSFX;
    public AudioSource moonMoveSFX;

    public GameObject head;
    public GameObject handle;
    public GameObject cam;
    public GameObject boss;
    public Sprite bossSurprisedSprite;
    GameObject player;

    Rigidbody2D playerRb;
    Rigidbody2D headRb;
    Rigidbody2D handleRb;
    Head headScript;
    Player playerScript;
    DistanceJoint2D distJoint;
    LineRenderer lineRenderer;
    PhysicsMaterial2D playerPhysMat;
    BoxCollider2D playerCollider;
    public GameObject[] specialUngrappleables;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        handleCanMove = true;

        headRb = head.GetComponent<Rigidbody2D>();
        handleRb = handle.GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        playerRb = player.GetComponent<Rigidbody2D>();
        headScript = head.GetComponent<Head>();
        distJoint = player.GetComponent<DistanceJoint2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        playerCollider = player.GetComponent<BoxCollider2D>();
        playerPhysMat = playerCollider.sharedMaterial;
        playerScript = player.GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGrappling && !headScript.IsWieldingObj) PointTowards(head.transform.position);
    }

    private void LateUpdate()
    {
        lineRenderer.SetPositions(new Vector3[] { handle.transform.position, head.transform.position });
    }

    //shoot and retract
    public IEnumerator RetractionShot()
    {
        StartCoroutine(Shoot());
        yield return new WaitUntil(() => isShooting);
        yield return new WaitUntil(() => !isShooting);
        if (hit)
        {
            if (headScript.attachedTo.CompareTag("Pullable"))
                StartCoroutine(Pull());
            else
                StartCoroutine(RetractRetractionShot());
        }
    }

    public IEnumerator MoonShot()
    {
        playerScript.DisableMoonShot();

        GameObject moon = GameObject.FindWithTag("Moon");
        PointTowards(moon.transform.position);
        handleCanMove = false;

        StartCoroutine(Shoot());

        //shrink head while shooting to make it look like it's far away
        yield return new WaitUntil(() => isShooting);
        while (isShooting)
        {
            head.transform.localScale /= headShrinkSpeedOnMoonShot;
            yield return new WaitForFixedUpdate();
        }

        if (hit)
        {
            StopAllAudio();
            //zipSFX.Play();
            moonMoveSFX.Play();
            moon.GetComponent<Rigidbody2D>().excludeLayers = LayerMask.GetMask("Everything");
            head.GetComponent<FixedJoint2D>().connectedAnchor = Vector2.zero;

            //make the boss surprised
            boss.GetComponent<SpriteRenderer>().sprite = bossSurprisedSprite;
            boss.transform.localScale = new(-boss.transform.localScale.x, boss.transform.localScale.y, boss.transform.localScale.z);
            boss.GetComponent<Animator>().enabled = false;

            for (int i = 0; i < moonPullDur; i++)
            {
                moon.transform.localScale += new Vector3(moonGrowSpeed, moonGrowSpeed);
                moon.transform.position = (Vector3.MoveTowards(moon.transform.position, handle.transform.position, moonPullSpeed));
                cam.transform.position = (Vector3.MoveTowards(cam.transform.position, cam.transform.position + Vector3.up * (moonPullDur * camRiseSpeed), camRiseSpeed));
                yield return new WaitForFixedUpdate();
            }

            //zipSFX.Stop();
            moonMoveSFX.Stop();
            FinishedMoonPull = true;
        }
    }

    void StopAllAudio()
    {
        AudioSource[] audioSources = (AudioSource[]) Resources.FindObjectsOfTypeAll(typeof(AudioSource));
        foreach (AudioSource source in audioSources)
        {
            source.Stop();
        }
    }

    IEnumerator RetractRetractionShot()
    {
        IsRetractingShot = true;
        zipSFX.Play();

        while (!ShouldEndRetraction && (head.transform.position - handle.transform.position).magnitude > closenessToHeadBeforeReconnecting)
        {
            playerRb.linearVelocity = (head.transform.position - handle.transform.position).normalized * retractionPower;
            yield return null;
        }

        distJoint.distance = (head.transform.position - handle.transform.position).magnitude;
        distJoint.enabled = true;

        yield return new WaitUntil(() => ShouldEndRetraction);
        ShouldEndRetraction = false;
        distJoint.enabled = false;
        IsRetractingShot = false;

        StartCoroutine(ReturnHead());
    }

    //shoot and swing
    public IEnumerator SwingShot()
    {
        StartCoroutine(Shoot());
        yield return new WaitUntil(() => isShooting);
        yield return new WaitUntil(() => !isShooting);
        if (hit)
        {
            StartCoroutine(Swing());
        }
    }

    IEnumerator Swing()
    {
        IsSwinging = true;
        distJoint.distance = (head.transform.position - handle.transform.position).magnitude;
        distJoint.enabled = true;
        if ((head.transform.position - handle.transform.position).x > 0)
        {
            playerRb.linearVelocity -= Vector2.Perpendicular(head.transform.position - handle.transform.position).normalized * swingPropulsion;
        }
        else
        {
            playerRb.linearVelocity += Vector2.Perpendicular(head.transform.position - handle.transform.position).normalized * swingPropulsion;
        }

        yield return new WaitUntil(() => ShouldEndSwing);
        EndSwing();
    }

    IEnumerator Shoot()
    {
        if (IsGrappling) yield break; //for safety

        //look in the correct direction
        if (playerScript.controllerPointer.activeSelf)
            playerScript.WhereToLook = playerScript.controllerPointer.transform.position;
        else
            playerScript.WhereToLook = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
        playerScript.Look();
        headScript.FollowHandle();

        if (Mathf.Abs(head.transform.eulerAngles.z - GetAngleTowards(head.transform.position, playerScript.WhereToLook)) > 1)
        {
            Debug.Log("head rotation: " + head.transform.eulerAngles.z);
            Debug.Log("should be: " + GetAngleTowards(head.transform.position, playerScript.WhereToLook));
        }

        IsGrappling = true;
        isShooting = true;
        shootSFX.Play();

        playerCollider.sharedMaterial = null; //get rid of the high friction so can slide around objects

        //shoot the head
        Vector2 headDir = new(Mathf.Cos(Mathf.Deg2Rad * head.transform.eulerAngles.z), Mathf.Sin(Mathf.Deg2Rad * head.transform.eulerAngles.z));
        headRb.linearVelocity = headDir * shotPower;

        //wait until the head hits a wall or goes too far
        Vector2 oldHeadVelocity = headRb.linearVelocity;
        yield return new WaitUntil(() => (oldHeadVelocity - headRb.linearVelocity).magnitude > 0
                                        || (handle.transform.position - head.transform.position).magnitude > maxShotDist);

        //if it went too far
        if ((handle.transform.position - head.transform.position).magnitude > maxShotDist)
        {
            StartCoroutine(ReturnHead());
        }
        else
        {
            GameObject attachTo = GetClosestGrappleableObject(head.transform.position, objectDetectionDist);
            hitSFX.Play();
            headScript.Attach(attachTo);

            hit = true;
        }

        isShooting = false;
    }

    GameObject GetClosestGrappleableObject(Vector3 origin, float maxDist)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, maxDist, Vector2.left, 0);

        GameObject closestGrappleableObject = this.gameObject;
        float closestGrappleableObjectDist = Mathf.Infinity;

        foreach (RaycastHit2D hit in hits)
        {
            Vector3 closestPoint = Physics2D.ClosestPoint(origin, hit.collider);

            if (
                IsGrappleable(hit) &&
                (closestPoint - origin).magnitude < closestGrappleableObjectDist
                )
            {
                closestGrappleableObject = hit.rigidbody.gameObject;
                closestGrappleableObjectDist = (closestPoint - origin).magnitude;
            }
        }

        return closestGrappleableObject;
    }

    bool IsGrappleable(RaycastHit2D hit)
    {
        foreach (GameObject go in specialUngrappleables)
        {
            if (hit.transform.gameObject == go)
            {
                return false;
            }
        }

        if (hit.collider == null || hit.collider.isTrigger)
            return false;

        return true;  
    }

    public void EndSwing()
    {
        distJoint.enabled = false;
        headScript.DetachAndDewield();
        playerRb.linearVelocity *= swingEndPropulsionMultiplier;
        IsSwinging = false;
        EndShot();
    }

    //forcefully stop all shots and reset related variables
    public void StopShot()
    {
        StopAllCoroutines();
        isShooting = false;
        ShouldEndRetraction = false;
        distJoint.enabled = false;
        headScript.DetachAndDewield();
        EndShot();
    }

    //for after a shot is completed
    void EndShot()
    {
        IsGrappling = false;
        ShouldEndSwing = false;
        IsRetractingShot = false;
        IsSwinging = false;
        hit = false;
        headScript.DetachAndDewield();

        zipSFX.Stop();

        playerCollider.sharedMaterial = playerPhysMat;

        //so the handle doesn't spin uncontrollably. It does that sometimes.
        handleRb.linearVelocity = Vector3.zero;
        handleRb.angularVelocity = 0;
        //so the head doesn't move weirdly on the next shot
        headRb.linearVelocity = Vector3.zero;
        headRb.angularVelocity = 0;

        //the update method in the Head script will teleport it back to the handle
    }

    IEnumerator ReturnHead()
    {
        headScript.DetachAndDewield();
        
        while ((head.transform.position - handle.transform.position).magnitude > closenessToHeadBeforeReconnecting)
        {
            head.transform.eulerAngles = new Vector3(0, 0, GetAngleTowards(head.transform.position, handle.transform.position) + 180);
            headRb.linearVelocity = (handle.transform.position - head.transform.position).normalized * headReturnSpeed;

            yield return null;
        }

        EndShot();
    }

    IEnumerator Pull()
    {
        while ((head.transform.position - handle.transform.position).magnitude > objClosenessBeforeWielding && !ShouldEndRetraction)
        {
            head.transform.eulerAngles = new Vector3(0, 0, GetAngleTowards(head.transform.position, handle.transform.position) + 180);
            headRb.linearVelocity = (handle.transform.position - head.transform.position).normalized * headReturnSpeed;

            yield return null;
        }

        //if pulled the object fully
        if ((head.transform.position - handle.transform.position).magnitude <= objClosenessBeforeWielding)
        {
            headScript.WieldAttachedObj();
        }

        //wait for the player to release the shot
        yield return new WaitUntil(() => ShouldEndRetraction);

        ShouldEndRetraction = false;
        EndShot();
    }

    public void PointTowards(Vector3 point)
    {
        if (handleCanMove)
        {
            handle.transform.eulerAngles = new Vector3(0, 0, GetAngleTowards(handle.transform.position, point));
        }
    }

    public static float GetAngleTowards(Vector3 origin, Vector3 destination)
    {
        Vector3 distFromPoint = destination - origin;
        float targetRotation = Mathf.Rad2Deg * Mathf.Atan2(distFromPoint.y, distFromPoint.x);
        if (targetRotation < 0)
        {
            targetRotation += 360;
        }
        return targetRotation;
    }
}
