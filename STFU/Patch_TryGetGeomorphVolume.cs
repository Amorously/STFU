using AIGraph;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace STFU;

[HarmonyPatch]
internal static class Patch_TryGetGeomorphVolume
{
    public static bool IsNotInLevel { get; private set; } = true;
    public const string DimensionIsBorken = "ERROR: Dimension X is borken";
    public const string NoCellInGrid = "ERROR: Couldn't find cell in grid";
    public const string BorkenCell = "ERROR: Borken cell";
    private const string Error = "Error";

    static Patch_TryGetGeomorphVolume()
    {
        LevelAPI.OnEnterLevel += OnEnterLevel;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
    }

    private static void OnEnterLevel()
    {
        foreach (var key in new[] { DimensionIsBorken, NoCellInGrid, BorkenCell })
        {
            if (Configuration.LogGeoVolumeResults && STFULogger.UniqueLogMap.TryGetValue(key, out var cnt))
            {
                STFULogger.LogWarning($"\"{key}\" message was logged {cnt} time(s)");
            }
        }

        IsNotInLevel = false;
    }

    private static void OnLevelCleanup()
    {
        IsNotInLevel = true;
    }

    [HarmonyTargetMethod]
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(AIG_GeomorphNodeVolume), nameof(AIG_GeomorphNodeVolume.TryGetGeomorphVolume));
    }

    [HarmonyTranspiler] // i wanted to learn transpilers and it works, shrugs
    private static IEnumerable<CodeInstruction> Transpile_GeomorphVolume(IEnumerable<CodeInstruction> instructions) 
    {
        var helper = AccessTools.Method(typeof(Patch_TryGetGeomorphVolume), nameof(TryGetGeomorphVolumeSilent), new[]
        {
            typeof(int),
            typeof(eDimensionIndex),
            typeof(Vector3),
            typeof(AIG_GeomorphNodeVolume).MakeByRefType()
        });

        yield return new(OpCodes.Ldarg_0); // floorNR
        yield return new(OpCodes.Ldarg_1); // dimensionIndex
        yield return new(OpCodes.Ldarg_2); // pos
        yield return new(OpCodes.Ldarg_3); // resultingGeoVolume
        yield return new(OpCodes.Call, helper); // call our helper method
        yield return new(OpCodes.Ret); // return
    }

    public static bool TryGetGeomorphVolumeSilent(int floorNR, eDimensionIndex dimensionIndex, Vector3 pos, out AIG_GeomorphNodeVolume resultingGeoVolume)
    {
        resultingGeoVolume = null!;

        var currentFloor = Builder.CurrentFloor;
        if (currentFloor == null)
        {
            Debug.LogError($"TryGetGeomorphVolume: No floor found! Dim: {dimensionIndex}. Doing this too early?");
            return false;
        }

        if (!currentFloor.GetDimension(dimensionIndex, out var dimension))
        {
            Debug.LogError($"TryGetGeomorphVolume: Tried to get dimension with index ({dimensionIndex}) but failed.");
            return false;
        }

        if (dimension.Grid == null || !TryGetCell(dimension.Grid, pos, out var cell))
        {
            LogErrorAs(DimensionIsBorken, dimension.DimensionIndex, dimension.Grid, pos, null);

            if (dimension.Grid != null)
            {
                LogErrorAs(NoCellInGrid, dimension.DimensionIndex, dimension.Grid, pos, null);
            }
            return false;
        }

        if (cell.m_grouping?.m_geoRoot == null)
        {
            LogErrorAs(BorkenCell, dimension.DimensionIndex, dimension.Grid, pos, cell);
            return false;
        }

        resultingGeoVolume = cell.m_grouping.m_geoRoot.m_nodeVolume.TryCast<AIG_GeomorphNodeVolume>()!;
        return resultingGeoVolume != null;
    }

    private static bool TryGetCell(LG_Grid grid, Vector3 pos, [MaybeNullWhen(false)] out LG_Cell cell)
    {
        pos -= grid.m_gridPosition;
        int x = Mathf.RoundToInt((pos.x - grid.m_cellDimHalf) / grid.m_cellDim);
        int z = Mathf.RoundToInt((pos.z - grid.m_cellDimHalf) / grid.m_cellDim);
        if (x < 0 || z < 0 || x >= grid.m_sizeX || z >= grid.m_sizeZ)
        {
            cell = null;
            return false;
        }
        cell = grid.GetCell(x, z);
        return true;
    }

    public static void LogErrorAs(string simpleStr, eDimensionIndex dim, LG_Grid? grid, Vector3 pos, LG_Cell? cell)
    {
        if (IsNotInLevel)
        {
            STFULogger.LogAs(Error, simpleStr, IsNotInLevel);
        }
        else
        {
            Debug.LogError(simpleStr switch
            {
                DimensionIsBorken => $"ERROR : Dimension {dim} is borken. grid: {grid}, cell: {cell}, DimIndexLookup: {dim}",
                NoCellInGrid => $"ERROR COULDN'T FIND CELL IN GRID : {dim}, gridPos: {grid?.m_gridPosition}, cellPos: {pos}",
                BorkenCell => $"ERROR : Borken cell (pos({pos}) | cellpos(x:{cell?.x}, z:{cell?.z})) grouping or root. grouping: {cell?.m_grouping?.ToString() ?? "<null>"} | dim: {dim}",
                _ => simpleStr
            });
        }
    }
}
