using System.Collections;
using System.IO;
using Pathfinding;
using Unity.VisualScripting;
using UnityEngine;

public class FollowingAstronaut : Astronaut
{
    AIPath aiPath;
    public float timeToUndetectPlayer;

    // Update is called once per frame
    void Update()
    {
        if (isInAttackMode)
            arm.transform.eulerAngles = new Vector3(0, 0, GrapplingHook.GetAngleTowards(transform.position, player.transform.position) + 180);
    }

    protected override void Start()
    {
        base.Start();
        aiPath = GetComponent<AIPath>();
        GetComponent<AIDestinationSetter>().target = player.transform;
        StartCoroutine(AttackOnSight());
    }

    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenShots);
            Shoot();
        }
    }

    IEnumerator ChangeAnimation()
    {
        if (!aiPath.reachedEndOfPath)
        {
            anim.SetBool("Flying", true);
            jetpackSFX.Play();
        }
        while (true)
        {
            yield return new WaitUntil(() => aiPath.reachedEndOfPath);
            anim.SetBool("Flying", false);
            jetpackSFX.Stop();
            yield return new WaitUntil(() => !aiPath.reachedEndOfPath);
            anim.SetBool("Flying", true);
            jetpackSFX.Play();

        }
    }

    IEnumerator AttackOnSight()
    {
        while (true)
        {
            //attack on sight
            yield return new WaitUntil(() =>
            (player.transform.position - transform.position).magnitude < playerDetectionDist &&
            !(Physics2D.Raycast(transform.position, player.transform.position - transform.position,
                                (player.transform.position - transform.position).magnitude, LayerMask.GetMask("Obstacle", "Wielded"))));

            isInAttackMode = true;
            aiPath.enabled = true;
            StartCoroutine(ChangeAnimation());

            //stop attacking when lose sight for too long
            while (true)
            {
                yield return new WaitUntil(() =>
                    (Physics2D.Raycast(transform.position, player.transform.position - transform.position,
                    (player.transform.position - transform.position).magnitude, LayerMask.GetMask("Obstacle", "Wielded"))));

                yield return new WaitForSeconds(timeToUndetectPlayer);

                if (Physics2D.Raycast(transform.position, player.transform.position - transform.position,
                                    (player.transform.position - transform.position).magnitude, LayerMask.GetMask("Obstacle", "Wielded")))
                    break;
            }

            isInAttackMode = false;
            aiPath.enabled = false;
            StopCoroutine(ChangeAnimation());
            anim.SetBool("Flying", false);
            jetpackSFX.Stop();
        }
    }
}
