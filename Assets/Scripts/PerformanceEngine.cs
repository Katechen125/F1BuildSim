using UnityEngine;

public static class PerformanceEngine
{
    private static readonly PartCategory[] DownforceCategories = { PartCategory.FrontWing, PartCategory.RearWing, PartCategory.Floor };
    private static readonly PartCategory[] DragCategories = { PartCategory.FrontWing, PartCategory.RearWing, PartCategory.Sidepods };
    private static readonly PartCategory[] PowerCategories = { PartCategory.Engine };

    // Tightened from 8/1.3 so mid-pack circuits stop clustering at OPTIMAL together.
    private const float MatchTolerance = 5f;
    private const float DecaySteepness = 1.6f;

    private static float AverageStat(CarBuild build, PartCategory[] categories, System.Func<CarPart, float> selector)
    {
        float total = 0f;
        int count = 0;
        foreach (var part in build.GetAllParts())
        {
            if (part == null) continue;
            bool inCategory = false;
            foreach (var cat in categories)
                if (part.category == cat) { inCategory = true; break; }
            if (!inCategory) continue;
            total += selector(part);
            count++;
        }
        return count == 0 ? 0f : total / count;
    }

    // Tyres matter most for real tyre wear/reliability, brakes and suspension
    // matter but less. A flat 3-way average diluted single-part swaps too
    // much (a Brakes or Suspension change barely moved the score), so tyres
    // get 50% weight and brakes/suspension split the remaining 50%.
    private static float ReliabilityWeightedAverage(CarBuild build)
    {
        float? tyres = build.tyres != null ? build.tyres.reliability : (float?)null;
        float? brakes = build.brakes != null ? build.brakes.reliability : (float?)null;
        float? suspension = build.suspension != null ? build.suspension.reliability : (float?)null;

        float totalWeight = 0f;
        float weightedSum = 0f;

        if (tyres.HasValue) { weightedSum += tyres.Value * 0.5f; totalWeight += 0.5f; }
        if (brakes.HasValue) { weightedSum += brakes.Value * 0.25f; totalWeight += 0.25f; }
        if (suspension.HasValue) { weightedSum += suspension.Value * 0.25f; totalWeight += 0.25f; }

        return totalWeight == 0f ? 0f : weightedSum / totalWeight;
    }

    private static float MatchScore(float actual, float target)
    {
        float diff = Mathf.Abs(actual - target);
        if (diff <= MatchTolerance) return 100f;
        float excess = diff - MatchTolerance;
        return Mathf.Clamp(100f - excess * DecaySteepness, 0f, 100f);
    }

    public static PerformanceResult Calculate(CarBuild build, Circuit circuit = null, bool requireComplete = true)
    {
        if (requireComplete && !build.IsComplete())
            return new PerformanceResult { isValid = false };

        float totalWeight = 0f;
        int partCount = 0;
        foreach (CarPart part in build.GetAllParts())
        {
            if (part == null) continue;
            totalWeight += part.weight;
            partCount++;
        }
        float avgWeight = partCount == 0 ? 0f : totalWeight / partCount;

        float avgDownforce = AverageStat(build, DownforceCategories, p => p.downforce);
        float avgDrag = AverageStat(build, DragCategories, p => p.drag);
        float avgPower = AverageStat(build, PowerCategories, p => p.power);
        float avgReliability = ReliabilityWeightedAverage(build);

        float aeroEfficiency = avgDrag > 0
            ? (avgDownforce / (avgDownforce + avgDrag)) * 100f
            : 100f;

        float speedScore = Mathf.Clamp((avgPower * 1.4f) - (avgDrag * 0.4f), 0f, 100f);
        float corneringScore = Mathf.Clamp(avgDownforce - (avgWeight * 0.2f), 0f, 100f);

        float lapTime = 90f
            - (speedScore * 0.12f)
            - (corneringScore * 0.08f)
            + (avgWeight * 0.04f);

        lapTime = Mathf.Clamp(lapTime, 60f, 90f);

        string circuitRating = "";
        float circuitFitScore = 0f;
        float downforceMatch = 0f, dragMatch = 0f, engineMatch = 0f, tyreMatch = 0f;

        if (circuit != null)
        {
            downforceMatch = MatchScore(avgDownforce, circuit.downforceRequirement);
            dragMatch = MatchScore(avgDrag, 100f - circuit.dragSensitivity);
            engineMatch = MatchScore(avgPower, circuit.engineDemand);
            tyreMatch = MatchScore(avgReliability, circuit.tyreStress);

            circuitFitScore = (downforceMatch * 0.35f)
                            + (dragMatch * 0.25f)
                            + (engineMatch * 0.25f)
                            + (tyreMatch * 0.15f);

            lapTime -= (circuitFitScore / 100f) * 8f;
            lapTime = Mathf.Clamp(lapTime, 60f, 90f);

            // Thresholds raised so OPTIMAL requires genuine circuit-specific tuning,
            // not just a decent all-round build.
            if (circuitFitScore >= 93f) circuitRating = "OPTIMAL SETUP";
            else if (circuitFitScore >= 78f) circuitRating = "GOOD SETUP";
            else if (circuitFitScore >= 58f) circuitRating = "AVERAGE SETUP";
            else circuitRating = "POOR SETUP";
        }

        float finalScore = circuit != null
            ? circuitFitScore
            : ((aeroEfficiency * 0.25f) + (speedScore * 0.25f)
             + (corneringScore * 0.25f) + (avgReliability * 0.25f));

        finalScore = Mathf.Clamp(finalScore, 0f, 100f);

        return new PerformanceResult
        {
            isValid = true,
            efficiencyScore = Mathf.Round(finalScore),
            lapTimeSeconds = Mathf.Round(lapTime * 10f) / 10f,
            aeroEfficiency = Mathf.Round(aeroEfficiency),
            speedScore = Mathf.Round(speedScore),
            corneringScore = Mathf.Round(corneringScore),
            reliability = Mathf.Round(avgReliability),
            circuitRating = circuitRating,
            circuitFitScore = Mathf.Round(circuitFitScore),
            avgPower = Mathf.Round(avgPower),
            avgDownforce = Mathf.Round(avgDownforce),
            avgDrag = Mathf.Round(avgDrag),
            downforceMatch = Mathf.Round(downforceMatch),
            dragMatch = Mathf.Round(dragMatch),
            engineMatch = Mathf.Round(engineMatch),
            tyreMatch = Mathf.Round(tyreMatch)
        };
    }
}

[System.Serializable]
public class PerformanceResult
{
    public bool isValid;
    public float efficiencyScore;
    public float circuitFitScore;
    public float lapTimeSeconds;
    public float aeroEfficiency;
    public float speedScore;
    public float corneringScore;
    public float reliability;
    public string circuitRating;
    public float avgPower;
    public float avgDownforce;
    public float avgDrag;

    public float downforceMatch;
    public float dragMatch;
    public float engineMatch;
    public float tyreMatch;
}