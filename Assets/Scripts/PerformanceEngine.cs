using UnityEngine;

public static class PerformanceEngine
{
    public static PerformanceResult Calculate(CarBuild build, Circuit circuit = null)
    {
        if (!build.IsComplete())
            return new PerformanceResult { isValid = false };

        float totalDownforce = 0f;
        float totalDrag = 0f;
        float totalWeight = 0f;
        float totalPower = 0f;
        float totalReliability = 0f;
        int partCount = 0;

        foreach (CarPart part in build.GetAllParts())
        {
            if (part == null) continue;
            totalDownforce += part.downforce;
            totalDrag += part.drag;
            totalWeight += part.weight;
            totalPower += part.power;
            totalReliability += part.reliability;
            partCount++;
        }

        float avgReliability = totalReliability / partCount;

        // Base scores
        float aeroEfficiency = totalDrag > 0
            ? (totalDownforce / (totalDownforce + totalDrag)) * 100f
            : 100f;

        float speedScore = Mathf.Clamp(
            (totalPower * 1.5f) - (totalDrag * 0.3f), 0f, 100f);

        float corneringScore = Mathf.Clamp(
            totalDownforce - (totalWeight * 0.3f), 0f, 100f);

        float lapTime = 90f
            - (speedScore * 0.15f)
            - (corneringScore * 0.1f)
            + (totalWeight * 0.05f);

        lapTime = Mathf.Clamp(lapTime, 60f, 90f);

        float efficiencyScore = (aeroEfficiency * 0.3f)
            + (speedScore * 0.3f)
            + (corneringScore * 0.2f)
            + (avgReliability * 0.2f);

        // Circuit modifier
        float circuitBonus = 0f;
        string circuitRating = "";

        if (circuit != null)
        {
            // Downforce match: how close is your downforce to what circuit needs
            float downforceNorm = totalDownforce / 3f; // normalise across 3 parts max ~300
            float downforceMatch = 100f - Mathf.Abs(downforceNorm - circuit.downforceRequirement);
            downforceMatch = Mathf.Clamp(downforceMatch, 0f, 100f);

            // Drag match: low drag circuits want low drag builds
            float dragNorm = totalDrag / 3f;
            float idealDrag = 100f - circuit.dragSensitivity;
            float dragMatch = 100f - Mathf.Abs(dragNorm - idealDrag);
            dragMatch = Mathf.Clamp(dragMatch, 0f, 100f);

            // Engine match: high engine demand circuits reward high power
            float powerNorm = totalPower;
            float engineMatch = 100f - Mathf.Abs(powerNorm - circuit.engineDemand);
            engineMatch = Mathf.Clamp(engineMatch, 0f, 100f);

            // Reliability vs tyre/brake stress
            float tyreMatch = 100f - Mathf.Abs(avgReliability - (100f - circuit.tyreStress));
            tyreMatch = Mathf.Clamp(tyreMatch, 0f, 100f);

            float overallMatch = (downforceMatch * 0.35f)
                               + (dragMatch * 0.25f)
                               + (engineMatch * 0.25f)
                               + (tyreMatch * 0.15f);

            circuitBonus = overallMatch * 0.25f;
            lapTime -= (overallMatch / 100f) * 8f;

            if (overallMatch >= 80f) circuitRating = "OPTIMAL SETUP";
            else if (overallMatch >= 60f) circuitRating = "GOOD SETUP";
            else if (overallMatch >= 40f) circuitRating = "AVERAGE SETUP";
            else circuitRating = "POOR SETUP";
        }

        efficiencyScore = Mathf.Clamp(efficiencyScore + circuitBonus, 0f, 100f);
        lapTime = Mathf.Clamp(lapTime, 60f, 90f);

        return new PerformanceResult
        {
            isValid = true,
            efficiencyScore = Mathf.Round(efficiencyScore),
            lapTimeSeconds = Mathf.Round(lapTime * 10f) / 10f,
            aeroEfficiency = Mathf.Round(aeroEfficiency),
            speedScore = Mathf.Round(speedScore),
            corneringScore = Mathf.Round(corneringScore),
            reliability = Mathf.Round(avgReliability),
            circuitRating = circuitRating
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
    public string circuitRating;
}