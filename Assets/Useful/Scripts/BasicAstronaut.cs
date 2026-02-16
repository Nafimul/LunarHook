using System.Collections;
using System.IO;
using Pathfinding;
using UnityEngine;

public class BasicAstronaut : Astronaut
{
    AIPath aiPath;

    // Update is called once per frame
    void Update()
    {
        if (isInAttackMode)
            arm.transform.eulerAngles = new Vector3(0, 0, GrapplingHook.GetAngleTowards(transform.position, player.transform.position));
    }

    protected override void Start()
    {
        base.Start();
        aiPath = GetComponent<AIPath>();
        StartCoroutine(StartAttackingOnSight());
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

    IEnumerator StartAttackingOnSight()
    {
        //wait until is close enough and can see the player
        yield return new WaitUntil(() =>
            (player.transform.position - transform.position).magnitude < playerDetectionDist &&
            !Physics2D.Raycast(transform.position, player.transform.position - transform.position,
                                (player.transform.position - transform.position).magnitude, LayerMask.GetMask("Obstacle", "Wielded")));

        isInAttackMode = true;
        aiPath.enabled = true;
        StartCoroutine(ChangeAnimation());
    }
}
