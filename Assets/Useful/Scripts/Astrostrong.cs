using System.Collections;
using UnityEngine;

public class Astrostrong : FollowingAstronaut
{
    public int shotsInARow;
    public float longBreakTime;
    protected override IEnumerator AttackLoop()
    {
        while (true)
        {
            for (int i = 0; i < shotsInARow; i++)
            {
                yield return new WaitForSeconds(timeBetweenShots);
                Shoot();
            }
            yield return new WaitForSeconds(longBreakTime);
        }
    }
}
