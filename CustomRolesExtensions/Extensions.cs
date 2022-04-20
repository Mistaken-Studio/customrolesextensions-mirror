﻿// -----------------------------------------------------------------------
// <copyright file="Extensions.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;

namespace Mistaken.API.CustomRoles
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

        /// <inheritdoc cref="CustomRole.RegisterRoles(bool, object)"/>
        public static IEnumerable<CustomRole> RegisterRoles()
        {
            List<CustomRole> registeredRoles = new List<CustomRole>();
            foreach (Type type in Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass).Where(x => x.GetInterface(nameof(IMistakenCustomRole)) != null))
            {
                if (!type.IsSubclassOf(typeof(CustomRole)) || type.GetCustomAttribute(typeof(CustomRoleAttribute)) is null)
                    continue;

                foreach (Attribute attribute in type.GetCustomAttributes(typeof(CustomRoleAttribute), true))
                {
                    try
                    {
                        CustomRole customRole = (CustomRole)Activator.CreateInstance(type);
                        customRole.Role = ((CustomRoleAttribute)attribute).RoleType;
                        customRole.GetType().GetMethod("TryRegister", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(customRole, new object[0]);
                        registeredRoles.Add(customRole);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }

            return registeredRoles;
        }

        /// <inheritdoc cref="CustomAbility.RegisterAbilities(bool, object)"/>
        public static IEnumerable<CustomAbility> RegisterAbilities()
        {
            List<CustomAbility> registeredAbilities = new List<CustomAbility>();
            foreach (Type type in Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass).Where(x => x.IsSubclassOf(typeof(CustomAbility))))
            {
                if (!type.IsSubclassOf(typeof(CustomAbility)) || type.GetCustomAttribute(typeof(CustomAbilityAttribute)) is null)
                    continue;

                foreach (Attribute attribute in type.GetCustomAttributes(typeof(CustomAbilityAttribute), true))
                {
                    try
                    {
                        CustomAbility customAbility = (CustomAbility)Activator.CreateInstance(type);
                        customAbility.GetType().GetMethod("TryRegister", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(customAbility, new object[0]);
                        registeredAbilities.Add(customAbility);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }

            return registeredAbilities;
        }
    }
}
