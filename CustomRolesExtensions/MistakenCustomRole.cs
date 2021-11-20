// -----------------------------------------------------------------------
// <copyright file="MistakenCustomRole.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using Mistaken.API.Extensions;

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

        /// <inheritdoc/>
        public override uint Id
        {
            get => (uint)this.CustomRole;
            set => _ = value;
        }

        /// <inheritdoc/>
        protected override void RoleAdded(Player player)
        {
            base.RoleAdded(player);
            if (this.BuiltInPermissions != KeycardPermissions.None)
                player.SetSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS, this.BuiltInPermissions);
        }

        /// <inheritdoc/>
        protected override void RoleRemoved(Player player)
        {
            base.RoleRemoved(player);
            if (this.BuiltInPermissions != KeycardPermissions.None)
                player.RemoveSessionVariable(SessionVarType.BUILTIN_DOOR_ACCESS);
        }
    }
}
