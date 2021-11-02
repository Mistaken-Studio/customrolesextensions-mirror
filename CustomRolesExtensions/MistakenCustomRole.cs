// -----------------------------------------------------------------------
// <copyright file="MistakenCustomRole.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Enums;
using Exiled.CustomRoles.API.Features;

namespace Mistaken.API.CustomRoles
{
    /// <inheritdoc/>
    public abstract class MistakenCustomRole : CustomRole, IMistakenCustomRole
    {
        /// <inheritdoc cref="CustomRole.Get(int)"/>
        public static MistakenCustomRole Get(MistakenCustomRoles id)
            => Get((int)id) as MistakenCustomRole;

        /// <inheritdoc cref="CustomRole.TryGet(int, out Exiled.CustomRoles.API.Features.CustomRole)"/>
        public static bool TryGet(MistakenCustomRoles id, out MistakenCustomRole customRole)
        {
            customRole = null;
            if (!TryGet((int)id, out var customRole2))
                return false;
            customRole = customRole2 as MistakenCustomRole;
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
    }
}
