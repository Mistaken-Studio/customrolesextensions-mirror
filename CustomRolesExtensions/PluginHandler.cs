// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;
using HarmonyLib;

namespace Mistaken.API.CustomRoles
{
    /// <inheritdoc/>
    public class PluginHandler : Plugin<Config>
    {
        /// <inheritdoc/>
        public override string Author => "Mistaken Devs";

        /// <inheritdoc/>
        public override string Name => "CustomRolesExtensions";

        /// <inheritdoc/>
        public override string Prefix => "MCRolesExtensions";

        /// <inheritdoc/>
        public override PluginPriority Priority => PluginPriority.Default;

        /// <inheritdoc/>
        public override Version RequiredExiledVersion => new Version(5, 0, 0);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            this.harmony = new Harmony("com.customrolesextensions.patch");
            this.harmony.PatchAll();
            base.OnEnabled();
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins += this.Register;
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            this.harmony.UnpatchAll();
            base.OnDisabled();
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins -= this.Register;
            this.UnRegister();
        }

        private static readonly List<CustomRole> RegisteredRoles = new List<CustomRole>();

        private static readonly List<CustomAbility> RegisteredAbilities = new List<CustomAbility>();

        private Harmony harmony;

        private void Register()
        {
            RegisteredAbilities.AddRange(Extensions.RegisterAbilities());
            foreach (var ability in RegisteredAbilities)
                Log.Debug($"Successfully registered {ability.Name} ({ability.AbilityType})", this.Config.VerbouseOutput);

            RegisteredRoles.AddRange(Extensions.RegisterRoles());
            foreach (var role in RegisteredRoles)
                Log.Debug($"Successfully registered {role.Name} ({role.Id})", this.Config.VerbouseOutput);
        }

        private void UnRegister()
        {
            foreach (var ability in CustomAbility.UnregisterAbilities(RegisteredAbilities))
            {
                Log.Debug($"Successfully unregistered {ability.Name} ({ability.AbilityType})", this.Config.VerbouseOutput);
                RegisteredAbilities.Remove(ability);
            }

            foreach (var role in CustomRole.UnregisterRoles(RegisteredRoles))
            {
                Log.Debug($"Successfully unregistered {role.Name} ({role.Id})", this.Config.VerbouseOutput);
                RegisteredRoles.Remove(role);
            }
        }
    }
}
