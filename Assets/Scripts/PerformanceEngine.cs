using UnityEngine;

public static class PerformanceEngine
{
    public static PerformanceResult Calculate(CarBuild build, Circuit circuit = null)
    {
        if (!build.IsComplete())
            return new PerformanceResult { isValid = false };

        float totalDownforce   = 0f;
        float totalDrag        = 0f;
        float totalWeight      = 0f;
        float totalPower       = 0f;
        float totalReliability = 0f;
        int   partCount        = 0;

        foreach (CarPart part in build.GetAllParts())
        {
            if (part == null) continue;
            totalDownforce   += part.downforce;
            totalDrag        += part.drag;
            totalWeight      += part.weight;
            totalPower       += part.power;
            totalReliability += part.reliability;
            partCount++;
        }

        float avgReliability = totalReliability / partCount;

        float aeroEfficiency = totalDrag > 0
            ? (totalDownforce / (totalDownforce + totalDrag)) * 100f
            : 100f;

        float speedScore = Mathf.Clamp(
            (totalPower * 1.2f) - (totalDrag * 0.4f), 0f, 100f);

        float corneringScore = Mathf.Clamp(
            totalDownforce - (totalWeight * 0.2f), 0f, 100f);

        float lapTime = 90f
            - (speedScore    * 0.12f)
            - (corneringScore * 0.08f)
            + (totalWeight   * 0.04f);

        lapTime = Mathf.Clamp(lapTime, 60f, 90f);

        float efficiencyScore = (aeroEfficiency  * 0.25f)
                              + (speedScore       * 0.25f)
                              + (corneringScore   * 0.25f)
                              + (avgReliability   * 0.25f);

        efficiencyScore = Mathf.Clamp(efficiencyScore, 0f, 100f);

        string circuitRating = "";

        if (circuit != null)
        {
            float downforceNorm = Mathf.Clamp(totalDownforce / 3f, 0f, 100f);
            float dragNorm      = Mathf.Clamp(totalDrag / 3f, 0f, 100f);
            float powerNorm     = Mathf.Clamp(totalPower, 0f, 100f);

            float downforceMatch = 100f - Mathf.Abs(downforceNorm - circuit.downforceRequirement);
            float dragMatch      = 100f - Mathf.Abs(dragNorm - (100f - circuit.dragSensitivity));
            float engineMatch    = 100f - Mathf.Abs(powerNorm - circuit.engineDemand);
            float tyreMatch      = 100f - Mathf.Abs(avgReliability - (100f - circuit.tyreStress));

            downforceMatch = Mathf.Clamp(downforceMatch, 0f, 100f);
            dragMatch      = Mathf.Clamp(dragMatch,      0f, 100f);
            engineMatch    = Mathf.Clamp(engineMatch,    0f, 100f);
            tyreMatch      = Mathf.Clamp(tyreMatch,      0f, 100f);

            float circuitMatch = (downforceMatch * 0.4f)
                               + (dragMatch      * 0.2f)
                               + (engineMatch    * 0.25f)
                               + (tyreMatch      * 0.15f);

            lapTime -= (circuitMatch / 100f) * 6f;
            lapTime  = Mathf.Clamp(lapTime, 60f, 90f);

            if      (circuitMatch >= 80f) circuitRating = "OPTIMAL SETUP";
            else if (circuitMatch >= 60f) circuitRating = "GOOD SETUP";
            else if (circuitMatch >= 40f) circuitRating = "AVERAGE SETUP";
            else                          circuitRating = "POOR SETUP";
        }

        return new PerformanceResult
        {
            isValid         = true,
            efficiencyScore = Mathf.Round(efficiencyScore),
            lapTimeSeconds  = Mathf.Round(lapTime * 10f) / 10f,
            aeroEfficiency  = Mathf.Round(aeroEfficiency),
            speedScore      = Mathf.Round(speedScore),
            corneringScore  = Mathf.Round(corneringScore),
            reliability     = Mathf.Round(avgReliability),
            circuitRating   = circuitRating
        };
    }
}

[System.Serializable]
public class PerformanceResult
{
    public bool   isValid;
    public float  efficiencyScore;
    public float  lapTimeSeconds;
    public float  aeroEfficiency;
    public float  speedScore;
    public float  corneringScore;
    public float  reliability;
    public string circuitRating;
}