using UnityEngine;

[CreateAssetMenu(menuName = "Delivery/Night Config", fileName = "Night_01")]
public class NightConfig : ScriptableObject
{
    public int nightNumber = 1;
    public string difficultyLabel = "Easy";

    [Header("Orders")]
    [Min(1)] public int minParcels = 1;
    [Min(1)] public int maxParcels = 2;

    [Header("Scares")]
    [Min(0)] public int jumpscares = 0;

    [Header("Time")]
    public float nightDurationSeconds = 300f;

    [Header("Rewards")]
    [Tooltip("Extra coins for finishing every order this night (can be 0).")]
    public int completionBonus = 0;
}