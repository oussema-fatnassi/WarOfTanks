using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchResultPayload
{
    public int winnerTeam;
    public int playerScore;
    public int aiScore;
    public float duration;
}