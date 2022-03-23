// -----------------------------------------------------------------------
// <copyright file="InternalDyingPatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.CustomRoles.API.Features;
using HarmonyLib;

namespace Mistaken.API.CustomRoles
{
    [HarmonyPatch(typeof(CustomRole), "OnInternalDying")]
    internal static class InternalDyingPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            yield return new CodeInstruction(OpCodes.Ret);
            yield break;
        }
    }
}
