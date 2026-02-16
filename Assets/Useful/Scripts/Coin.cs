using UnityEngine;

public class Coin : MonoBehaviour
{
    public State state;
    public int number;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //deactivate if has already been collected
        foreach (int num in state.coinNumsCollected)
        {
            if (number == num)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
