using UnityEngine;
using System.Collections.Generic;

public static class BuildAdvisor
{
    public class Suggestion
    {
        public PartCategory category;
        public CarPart currentPart;
        public CarPart suggestedPart;
        public float currentScore;
        public float projectedScore;
        public float gain;
    }

    public static List<Suggestion> GetTopSuggestions(CarBuild build, Circuit circuit, CarPart[] allParts, int maxSuggestions = 2)
    {
        var suggestions = new List<Suggestion>();
        if (build == null || circuit == null || allParts == null) return suggestions;

        float baseScore = PerformanceEngine.Calculate(build, circuit, requireComplete: false).circuitFitScore;

        foreach (PartCategory category in System.Enum.GetValues(typeof(PartCategory)))
        {
            CarPart original = GetPart(build, category);
            CarPart bestAlt = null;
            float bestScore = baseScore;

            foreach (var candidate in allParts)
            {
                if (candidate == null || candidate.category != category || candidate == original)
                    continue;

                SetPart(build, category, candidate);
                float trialScore = PerformanceEngine.Calculate(build, circuit, requireComplete: false).circuitFitScore;

                if (trialScore > bestScore)
                {
                    bestScore = trialScore;
                    bestAlt = candidate;
                }
            }

            // Always revert, we're only probing
            SetPart(build, category, original);

            if (bestAlt != null)
            {
                suggestions.Add(new Suggestion
                {
                    category = category,
                    currentPart = original,
                    suggestedPart = bestAlt,
                    currentScore = baseScore,
                    projectedScore = bestScore,
                    gain = bestScore - baseScore
                });
            }
        }

        suggestions.Sort((a, b) => b.gain.CompareTo(a.gain));
        if (suggestions.Count > maxSuggestions)
            suggestions.RemoveRange(maxSuggestions, suggestions.Count - maxSuggestions);

        return suggestions;
    }

    static CarPart GetPart(CarBuild b, PartCategory c) => c switch
    {
        PartCategory.FrontWing => b.frontWing,
        PartCategory.RearWing => b.rearWing,
        PartCategory.Engine => b.engine,
        PartCategory.Suspension => b.suspension,
        PartCategory.Tyres => b.tyres,
        PartCategory.Brakes => b.brakes,
        PartCategory.Sidepods => b.sidepods,
        PartCategory.Floor => b.floor,
        _ => null
    };

    static void SetPart(CarBuild b, PartCategory c, CarPart part)
    {
        switch (c)
        {
            case PartCategory.FrontWing: b.frontWing = part; break;
            case PartCategory.RearWing: b.rearWing = part; break;
            case PartCategory.Engine: b.engine = part; break;
            case PartCategory.Suspension: b.suspension = part; break;
            case PartCategory.Tyres: b.tyres = part; break;
            case PartCategory.Brakes: b.brakes = part; break;
            case PartCategory.Sidepods: b.sidepods = part; break;
            case PartCategory.Floor: b.floor = part; break;
        }
    }
}