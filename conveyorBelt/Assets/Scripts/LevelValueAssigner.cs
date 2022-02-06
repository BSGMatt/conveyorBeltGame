using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class for modifying the values of the PackageGenerator based on the current Level. 
/// </summary>
public static class LevelValueAssigner
{
    private static int NUM_HARDCODED_LEVELS = 0;

    /// <summary>
    /// Minimum package speeds for levels 1, 2, and 3. 
    /// </summary>
    private static float[] startingMinSpeeds = {1.1f, 1.2f, 2};

    /// <summary>
    /// Maximum package speeds for levels 1, 2, and 3. 
    /// </summary>
    private static float[] startingMaxSpeeds = {3.1f, 3.2f, 3}; //3.1f

    private static float[] startingInitPackageRates = {1.4f, 1.35f, 1.3f};

    private static float[] startingMinPackageRates = {0.65f, 0.62f, 0.58f};

    public static void AssignLevelValues(PackageGenerator pg, int level) {

        //If level = 0, keep initial values
        if (level <= 0) {
            pg.SetPackageSpeedRange(startingMinSpeeds[0], startingMaxSpeeds[0]);
            pg.SetInitPackageRate(startingInitPackageRates[0]);
            pg.SetMinPackageRate(startingMinPackageRates[0]);
            return;
        }

        //Check if there are hardcoded values for this level.
        if (level <= NUM_HARDCODED_LEVELS) {
            pg.SetPackageSpeedRange(startingMinSpeeds[level - 1], startingMaxSpeeds[level - 1]);
            pg.SetInitPackageRate(startingInitPackageRates[level - 1]);
            pg.SetMinPackageRate(startingMinPackageRates[level - 1]);
        }
        else {
            //If not, use the formulas. 
            pg.SetPackageSpeedRange(MinPackageSpeedFormula(level), MaxPackageSpeedFormula(level));
            pg.SetInitPackageRate(InitPackageRateFormula(level));
            pg.SetMinPackageRate(MinPackageRateFormula(level));
        }

        
    }

    private static float MinPackageSpeedFormula(int level) {
        return   (0.4f * level) + startingMinSpeeds[2];
    }

    private static float MaxPackageSpeedFormula(int level) {
        return (0.0044f * (level * level)) + (0.7624f * level) + startingMaxSpeeds[2];
    }

    private static float MinPackageRateFormula(int level) {
        return 0.5f;
    }

    private static float InitPackageRateFormula(int level) {
        float value = ((0.0013f * (level * level)) - (0.0636f * level) + 1f);
        return value < 0.5f ? 0.5f : value; 
    }
}
