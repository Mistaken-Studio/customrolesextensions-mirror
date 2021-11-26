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

        /// <summary>
        /// Gets Keycard permissins for bulitin door permission session var.
        /// </summary>
        public virtual KeycardPermissions BuiltInPermissions { get; } = KeycardPermissions.None;

        /// <summary>
        /// 
        /// </summary>
        public virtual string DisplayName { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual Dictionary<ItemType, ushort> Ammo { get; }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool SetLatestUnitName { get; } = false;

        /// <inheritdoc/>
        public override uint Id
        {
            get => (uint)this.CustomRole;
            set => _ = value;
        }

        /// <inheritdoc/>
        public override void AddRole(Player player)
        {
            if (this.SetLatestUnitName)
            {
                var prevRole = player.Role;
                var old = Respawning.RespawnManager.CurrentSequence();
                Respawning.RespawnManager.Singleton._curSequence = Respawning.RespawnManager.RespawnSequencePhase.SpawningSelectedTeam;
                player.Role = this.Role == RoleType.None ? RoleType.ClassD : this.Role;
                player.ReferenceHub.characterClassManager.NetworkCurSpawnableTeamType = 2;
                player.UnitName = Respawning.RespawnManager.Singleton.NamingManager.AllUnitNames.Last().UnitName;
                Respawning.RespawnManager.Singleton._curSequence = old;
                if (this.Role == RoleType.None)
                    player.Role = prevRole;
            }

            base.AddRole(player);
        }

        /// <inheritdoc/>
        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            player.InfoArea = ~PlayerInfoArea.Role;
            if (this.BuiltInPermissions != KeycardPermissions.None)
                player.SetSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS, this.BuiltInPermissions);

            if (this.Ammo != null)
            {
                foreach (var item in player.Ammo.Keys)
                    player.Ammo[item] = 0;

                foreach (var item in this.Ammo)
                    player.Ammo[item.Key] = item.Value;
            }

            if (!string.IsNullOrWhiteSpace(this.DisplayName))
            {
                Mistaken.API.CustomInfoHandler.Set(player, $"custom-role-{this.Name}", this.DisplayName);
                player.SetGUI($"custom-role-{this.Name}", GUI.PseudoGUIPosition.BOTTOM, $"<color=yellow>Grasz</color> jako {this.DisplayName}");
                player.SetGUI($"custom-role-{this.Name}-descripton", GUI.PseudoGUIPosition.MIDDLE, this.Description);
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

            Mistaken.API.CustomInfoHandler.Set(player, $"custom-role-{this.Name}", null);
            player.SetGUI($"custom-role-{this.Name}", GUI.PseudoGUIPosition.BOTTOM, null);
            RLogger.Log("CUSTOM CLASSES", this.Name, $"{player.PlayerToString()} is no longer {this.Name}");
        }
    }
}
