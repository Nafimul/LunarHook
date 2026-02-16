using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "State", menuName = "Scriptable Objects/State")]

public class State : ScriptableObject
{
    public Vector2 spawnpoint;
    public ArrayList coinNumsCollected = new();
    public Vector2 lastCheckpoint;
    public int lastCheckpointNum;
    public Vector2 secondToLastCheckpoint;
    public int secondToLastCheckpointNum;
    public bool hasDied;

    public InputSystem_Actions playerControls;

}
