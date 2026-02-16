using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding;
using Unity.Burst.Intrinsics;
using UnityEngine;

public abstract class Astronaut : MonoBehaviour
{

    public float bulletSpeed;
    public float timeBetweenShots;
    public float playerDetectionDist;
    public int defaultHealth;
    public float invincibilityFramesDuration;
    public float flashesPerSecWhileInvincible;
    public float minDamagingObjSpeed;
    public int damageTakenFromObj;

    protected GameObject player;
    protected FixedJoint2D grappleHeadJoint;
    protected GrapplingHook grappleScript;
    protected BoxCollider2D coll;
    protected Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Animator anim;
    public AudioSource shootSFX;
    public AudioSource damagedSFX;
    public AudioSource deathSFX;
    public AudioSource jetpackSFX;
    public GameObject arm;
    public Door[] assignedDoors;

    protected bool isInAttackMode;
    protected int health;
    protected bool invincible;
    protected bool isDying;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    virtual protected void Start()
    {
        grappleHeadJoint = GameObject.FindWithTag("GrappleHead").GetComponent<FixedJoint2D>();
        grappleScript = GameObject.FindWithTag("GrapplingHook").GetComponent<GrapplingHook>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        coll = GetComponent<BoxCollider2D>();

        health = defaultHealth;
        if (assignedDoors != null)
        {
            foreach (Door door in assignedDoors) 
            {
                door.AddEnemyToKill();
            }
        }

        StartCoroutine(AttackLoop());
    }

    protected abstract IEnumerator AttackLoop();

    protected void Shoot()
    {
        if (isInAttackMode && !isDying)
        {
            GameObject bullet = ObjectPool.SharedInstance.GetPooledObject();

            if (bullet != null)
            {
                bullet.transform.SetPositionAndRotation(transform.position, transform.rotation);
                bullet.GetComponent<Bullet>().Activate();
                bullet.GetComponent<Rigidbody2D>().linearVelocity = (player.transform.position - bullet.transform.position).normalized * bulletSpeed;
            }

            shootSFX.PlayOneShot(shootSFX.clip, 1);
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GetDamaged(collision.gameObject.GetComponent<Player>().collisionDamage);
        }
        else if (collision.gameObject.CompareTag("GrappleHead"))
        {
            GetDamaged(collision.gameObject.GetComponent<Head>().collisionDamage);
        }
        //if the player shot a sufficiently fast object at this person
        else if (collision.gameObject.CompareTag("Pullable") &&
                collision.gameObject.GetComponent<Rigidbody2D>().linearVelocity.magnitude > minDamagingObjSpeed
                && collision.gameObject.layer != LayerMask.NameToLayer("Wielded"))
        {
            GetDamaged(damageTakenFromObj);
        }
    }

    void GetDamaged(int damage)
    {
        if (!invincible && !isDying)
        {
            health -= damage;
            if (health <= 0)
                StartCoroutine(Die());
            else
            {
                damagedSFX.Play();
                StartCoroutine(InvincibilityFrames());
            }
        }
    }

    IEnumerator Die()
    {
        if (!invincible)
        {
            isDying = true;

            //boost the player if they killed the astronaut by touch and isn't swinging
            bool shouldBoostPlayer = coll.IsTouching(player.GetComponent<Collider2D>()) && !grappleScript.IsSwinging;

            //disable all colliders
            coll.enabled = false;
            GameObject[] children = GetChildrenRecursively(gameObject).ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].TryGetComponent<Collider2D>(out Collider2D collider))
                {
                    collider.enabled = false;
                }
            }

            if (shouldBoostPlayer)
                player.GetComponent<Player>().KillSpeedBoost(transform.position);

            //let any assigned doors know that they're dead
            if (assignedDoors != null)
            {
                foreach (Door door in assignedDoors)
                {
                    door.RemoveEnemyToKill();
                }
            }

            deathSFX.Play();
            anim.SetTrigger("Died");

            //wait for the death animation to end
            yield return null;
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length - Time.deltaTime);
            
            Destroy(gameObject);
        }
    }

    List<GameObject> GetChildrenRecursively(GameObject parent)
    {
        List<GameObject> children = new();

        foreach (Transform child in parent.transform)
        {
            children.Add(child.gameObject);
            children.AddRange(GetChildrenRecursively(child.gameObject));
        }

        return children;
    }

    IEnumerator InvincibilityFrames()
    {
        if (invincibilityFramesDuration > 0)
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
        else //just flash once and don't become invisible
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds((1 / flashesPerSecWhileInvincible) / 2);
            spriteRenderer.enabled = true;
        }
    }
}
