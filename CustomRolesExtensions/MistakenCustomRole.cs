// -----------------------------------------------------------------------
// <copyright file="MistakenCustomRole.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Mistaken.API.Extensions;
using Mistaken.RoundLogger;

namespace Mistaken.API.CustomRoles
{
    /// <inheritdoc/>
    public abstract class MistakenCustomRole : CustomRole, IMistakenCustomRole
    {
        /// <inheritdoc cref="CustomRole.Get(int)"/>
        public static MistakenCustomRole Get(MistakenCustomRoles id)
            => Get((int)id) as MistakenCustomRole;

        /// <inheritdoc cref="CustomRole.Get(int)"/>
        public static T Get<T>(MistakenCustomRoles id)
            where T : MistakenCustomRole, new()
            => Get((int)id) as T;

        /// <inheritdoc cref="CustomRole.TryGet(int, out Exiled.CustomRoles.API.Features.CustomRole)"/>
        public static bool TryGet(MistakenCustomRoles id, out MistakenCustomRole customRole)
        {
            customRole = null;
            if (!TryGet((int)id, out var customRole2))
                return false;
            customRole = customRole2 as MistakenCustomRole;
            return true;
        }

        /// <inheritdoc cref="CustomRole.TryGet(int, out Exiled.CustomRoles.API.Features.CustomRole)"/>
        public static bool TryGet<T>(MistakenCustomRoles id, out T customRole)
            where T : MistakenCustomRole, new()
        {
            customRole = null;
            if (!TryGet((int)id, out var customRole2))
                return false;
            customRole = customRole2 as T;
            return true;
        }

        /// <inheritdoc/>
        public abstract MistakenCustomRoles CustomRole { get; }

        /// <inheritdoc/>
        public override uint Id
        {
            get => (uint)this.CustomRole;
            set => _ = value;
        }

        /// <inheritdoc/>
        public override string CustomInfo { get; set; }

        /// <inheritdoc/>
        public override void AddRole(Player player)
        {
            if (this.SetLatestUnitName)
            {
                var prevRole = player.Role;
                var old = Respawning.RespawnManager.CurrentSequence();
                Respawning.RespawnManager.Singleton._curSequence = Respawning.RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
                player.Role.Type = this.Role == RoleType.None ? RoleType.ClassD : this.Role;
                player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = 2;
                player.UnitName = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Last().UnitName;
                Respawning.RespawnManager.Singleton._curSequence = old;
                if (this.Role == RoleType.None)
                    player.Role = prevRole;
            }

            base.AddRole(player);
        }

        /// <summary>
        /// Gets Keycard permissins for bulitin door permission session var.
        /// </summary>
        protected virtual KeycardPermissions BuiltInPermissions { get; } = KeycardPermissions.None;

        /// <summary>
        /// Gets a value indicating whether role grants infinite ammo.
        /// </summary>
        protected virtual bool InfiniteAmmo { get; } = false;

        /// <summary>
        /// Gets name used to for GUI.
        /// </summary>
        protected virtual string DisplayName { get; }

        /// <summary>
        /// Gets ammo set when role is added.
        /// </summary>
        protected virtual Dictionary<ItemType, ushort> Ammo { get; }

        /// <summary>
        /// Gets a value indicating whether after adding role latest unit.
        /// </summary>
        protected virtual bool SetLatestUnitName { get; } = false;

        /// <inheritdoc/>
        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            player.InfoArea = ~PlayerInfoArea.Role;
            if (this.BuiltInPermissions != KeycardPermissions.None)
                player.SetSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS, this.BuiltInPermissions);
            if (this.InfiniteAmmo)
            {
                player.SetSessionVariable(SessionVarType.INFINITE_AMMO, true);
                Diagnostics.Module.CallSafeDelayed(
                2f,
                () =>
                {
                    player.Ammo[ItemType.Ammo12gauge] = 1;
                    player.Ammo[ItemType.Ammo44cal] = 1;
                    player.Ammo[ItemType.Ammo556x45] = 1;
                    player.Ammo[ItemType.Ammo762x39] = 1;
                    player.Ammo[ItemType.Ammo9x19] = 1;
                },
                "AddAmmo");
            }
            else
            {
                Diagnostics.Module.CallSafeDelayed(
                    2f,
                    () =>
                    {
                        if (this.Ammo != null)
                        {
                            foreach (var item in player.Ammo.Keys.ToArray())
                                player.Ammo[item] = 0;

                            foreach (var item in this.Ammo)
                                player.Ammo[item.Key] = item.Value;
                        }
                    },
                    "AddAmmo");
            }

            if (!string.IsNullOrWhiteSpace(this.DisplayName))
            {
                Mistaken.API.CustomInfoHandler.Set(player, $"custom-role-{this.Name}", this.DisplayName);
                player.SetGUI($"custom-role-{this.Name}", GUI.PseudoGUIPosition.BOTTOM, $"<color=yellow>Grasz</color> jako {this.DisplayName}");
                player.SetGUI($"custom-role-{this.Name}-descripton", GUI.PseudoGUIPosition.MIDDLE, this.Description, 15f);
            }

            RLogger.Log("CUSTOM CLASSES", this.Name, $"Spawned {player.PlayerToString()} as {this.Name}");
        }

        /// <inheritdoc/>
        protected override void RoleRemoved(Player player)
        {
            base.RoleRemoved(player);

            if (!player.GetCustomRoles().Any())
                player.InfoArea |= PlayerInfoArea.Role;

            if (this.BuiltInPermissions != KeycardPermissions.None)
                player.RemoveSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS);
            if (this.InfiniteAmmo)
                player.RemoveSessionVariable(SessionVarType.INFINITE_AMMO);

            Mistaken.API.CustomInfoHandler.Set(player, $"custom-role-{this.Name}", null);
            player.SetGUI($"custom-role-{this.Name}", GUI.PseudoGUIPosition.BOTTOM, null);
            RLogger.Log("CUSTOM CLASSES", this.Name, $"{player.PlayerToString()} is no longer {this.Name}");
        }
    }
}
