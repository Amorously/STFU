using GTFO.API;
using UnityEngine;

namespace STFU;

internal static class BoxColliderPseudofix
{
    public static readonly Dictionary<BoxCollider, (Vector3, Vector3)> AffectedBoxColliders = new();
    public const string NegativeScaleSize = "BoxColliders does not support negative size or scale.";
    private const string Warning = "Warning";

    public static void Setup()
    {
        LevelAPI.OnBuildDone += OnBuildDone;
        LevelAPI.OnEnterLevel += OnEnterLevel;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
    }

    private static void OnBuildDone()
    {
        foreach (var box in UnityEngine.Object.FindObjectsOfType<BoxCollider>())
        {
            if (box != null)
            {
                Transform trans = box.transform;
                Vector3 ws = trans.lossyScale;
                Vector3 size = box.size;
                bool badScale = (ws.x < 0.0f) || (ws.y < 0.0f) || (ws.z < 0.0f);
                bool badSize = (size.x < 0.0f) || (size.y < 0.0f) || (size.z < 0.0f);

                if (badScale || badSize)
                {
                    AffectedBoxColliders[box] = (trans.localScale, size);
                    STFULogger.LogAs(Warning, NegativeScaleSize, true); // just in case i do something with this later
                    Vector3 ls = trans.localScale;
                    ls.x *= Mathf.Sign(ws.x);
                    ls.y *= Mathf.Sign(ws.y);
                    ls.z *= Mathf.Sign(ws.z);
                    trans.localScale = ls;
                    box.size = new(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
                }
            }
        }
    }

    private static void OnEnterLevel()
    {
        foreach (var kvp in AffectedBoxColliders) // return original size/scale
        {
            var box = kvp.Key;
            var (origScale, origSize) = kvp.Value;
            box.transform.localScale = origScale;
            box.size = origSize;
        }

        if (Configuration.LogBoxColliderResults && AffectedBoxColliders.Any())
        {
            STFULogger.LogWarning($"Found {AffectedBoxColliders.Count} BoxColliders with negative size or scale");

            if (Configuration.ShowAffectedBoxColliderNames)
            {
                STFULogger.LogDebug(AffectedBoxColliders.Keys.Select(box => box.name).Aggregate((a, b) => a + ", " + b));
            }
        }        
    }

    private static void OnLevelCleanup()
    {
        AffectedBoxColliders.Clear();
    }
}
