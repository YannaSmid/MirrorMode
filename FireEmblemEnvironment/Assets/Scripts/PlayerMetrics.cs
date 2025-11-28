using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerMetrics : MonoBehaviour
{
    // Start is called before the first frame updateprivate string filePath;
    private string filePath;
    public string playerID = "P00";

    int round = 0;
    int totalKills = 0;
    int totalDeaths = 0;
    int totalWins = 0;
    int totalAttacks = 0;
    int totalMovements = 0;
    int effective = 0;
    int advantage = 0;
    int disadvantage = 0;



    void Awake()
    {
        InitializeLogger();
    }
    public void InitializeLogger()
    {
        string folderPath = Path.Combine(Application.dataPath, "MetricsPlayerTests");

        // Ensure the folder exists
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Construct the file name
        string fileName = $"Player_{playerID}.txt";
        filePath = Path.Combine(folderPath, fileName);

        // If the file exists, add "Copy" to the name
        int copyIndex = 1;
        while (File.Exists(filePath))
        {
            fileName = $"Player_{playerID}_Copy{copyIndex}.txt";
            filePath = Path.Combine(folderPath, fileName);
            copyIndex++;
        }

        // Create the new log file
        using (FileStream fs = File.Create(filePath))
        {
            Debug.Log($"New log file created: {filePath}");
        }
    }

    public void AddActions(int unit, int attack, int movement)
    {
        totalAttacks = attack;
        totalMovements = movement;

        LogMetrics(unit);


    }

    // Log the metrics each episode

    public void LogEndRoundMetrics(int r, int kills, int deaths, int win)
    {
        round = r;
        totalKills = kills;
        totalDeaths = deaths;
        totalWins = win;
    }

    public void LogCombatInfo(bool eff, bool adv, bool disadv)
    {
        if (eff)
        {
            effective += 1;
        }

        else if (adv)
        {
            advantage += 1;
        }

        else if (disadv)
        {
            disadvantage += 1;
        }
    }
    public void LogMetrics(int unit)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"[UNIT: {unit} round {round}] Kills: {totalKills}, deaths: {totalDeaths}, wins: {totalWins}, attacks: {totalAttacks}, movements: {totalMovements}, Effectives: {effective}, advantages: {advantage}, disadvantages: {disadvantage}");
            }

            effective = 0;
            advantage = 0;
            disadvantage = 0;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write to file: {e.Message}");
        }
    }
}
