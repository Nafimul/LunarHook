using System.Collections;
using Pathfinding;
using UnityEngine;

public class Astrosword : Astronaut
{
    public float rotationSpeed;

    protected override void Start()
    {
        base.Start();
        anim.SetBool("Flying", true);
    }


    protected override IEnumerator AttackLoop()
    {
        //just spins
        while (true)
        {
            transform.eulerAngles = transform.eulerAngles + new Vector3(0, 0, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
