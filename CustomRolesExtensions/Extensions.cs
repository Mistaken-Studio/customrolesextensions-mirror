// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.CustomItems.API.Features;
using Exiled.CustomRoles.API.Features;
using Footprinting;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;
using Mirror;
using UnityEngine;

namespace Mistaken.CustomRolesExtensions
{
    /// <summary>
    /// Custom Ranks Extensions.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Checks if <paramref name="player"/> has custom role made by Mistaken Devs with id <paramref name="roleType"/>.
        /// </summary>
        /// <param name="player">Player to check.</param>
        /// <param name="roleType">Role Id.</param>
        /// <returns>If player has specified custom role.</returns>
        public static bool HasCustomRole(this Player player, MistakenCustomRoles roleType)
        {
            if (!CustomRole.TryGet(player, out var roles))
                return false;

            foreach (var role in roles)
            {
                if (!(role is IMistakenCustomRole mistakenRole))
                    continue;
                if (mistakenRole.CustomRole == roleType)
                    return true;
            }

            return false;
        }

        /// <inheritdoc cref="MistakenCustomRole.Get(MistakenCustomRoles)"/>
        public static MistakenCustomRole Get(this MistakenCustomRoles id)
            => MistakenCustomRole.Get(id);

        /// <inheritdoc cref="MistakenCustomRole.TryGet(MistakenCustomRoles, out MistakenCustomRole)"/>
        public static bool TryGet(this MistakenCustomRoles id, out MistakenCustomRole customRole)
            => MistakenCustomRole.TryGet(id, out customRole);
    }
}
