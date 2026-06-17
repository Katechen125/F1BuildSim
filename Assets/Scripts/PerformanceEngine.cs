using UnityEngine;

public static class PerformanceEngine
{
    public static PerformanceResult Calculate(CarBuild build)
    {
        if (!build.IsComplete())
            return new PerformanceResult { isValid = false };

        // Combine all stats across selected parts
        float totalDownforce = 0f;
        float totalDrag = 0f;
        float totalWeight = 0f;
        float totalPower = 0f;
        float totalReliability = 0f;
        int partCount = 0;

        foreach (CarPart part in build.GetAllParts())
        {
            if (part == null) continue;
            totalDownforce  += part.downforce;
            totalDrag       += part.drag;
            totalWeight     += part.weight;
            totalPower      += part.power;
            totalReliability += part.reliability;
            partCount++;
        }

        float avgReliability = totalReliability / partCount;

        // Aero efficiency: how much downforce per unit of drag
        float aeroEfficiency = totalDrag > 0 
            ? (totalDownforce / (totalDownforce + totalDrag)) * 100f 
            : 100f;

        // Speed score: power minus drag penalty
        float speedScore = Mathf.Clamp((totalPower * 1.2f) - (totalDrag * 0.5f), 0f, 100f);

        // Cornering score: downforce minus weight penalty
        float corneringScore = Mathf.Clamp(totalDownforce - (totalWeight * 0.3f), 0f, 100f);

        // Estimated lap time: lower is better
        // Base time 90 seconds, improved by speed and cornering
        float lapTime = 90f 
            - (speedScore * 0.15f) 
            - (corneringScore * 0.1f) 
            + (totalWeight * 0.05f);

        lapTime = Mathf.Clamp(lapTime, 60f, 90f);

        // Overall efficiency score out of 100
        float efficiencyScore = (aeroEfficiency * 0.3f)
            + (speedScore * 0.3f)
            + (corneringScore * 0.2f)
            + (avgReliability * 0.2f);

        efficiencyScore = Mathf.Clamp(efficiencyScore, 0f, 100f);

        return new PerformanceResult
        {
            isValid         = true,
            efficiencyScore = Mathf.Round(efficiencyScore),
            lapTimeSeconds  = Mathf.Round(lapTime * 10f) / 10f,
            aeroEfficiency  = Mathf.Round(aeroEfficiency),
            speedScore      = Mathf.Round(speedScore),
            corneringScore  = Mathf.Round(corneringScore),
            reliability     = Mathf.Round(avgReliability)
        };
    }
}

[System.Serializable]
public class PerformanceResult
{
    public bool isValid;
    public float efficiencyScore;
    public float lapTimeSeconds;
    public float aeroEfficiency;
    public float speedScore;
    public float corneringScore;
    public float reliability;
}