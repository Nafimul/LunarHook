using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public State state;

    private void Awake()
    {
        if (Instance == null)
        {
            ResetState();
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public void ResetState()
    {
        state.spawnpoint = Vector2.zero;
        state.coinNumsCollected = new();
        state.lastCheckpoint = Vector2.zero;
        state.lastCheckpointNum = 0;
        state.secondToLastCheckpoint = Vector2.zero;
        state.secondToLastCheckpointNum = 0;
        state.hasDied = false;
    }
}
