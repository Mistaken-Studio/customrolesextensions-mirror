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
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins += this.CustomEvents_LoadedPlugins;
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            this.harmony.UnpatchAll();
            base.OnDisabled();
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins -= this.CustomEvents_LoadedPlugins;
            this.UnRegister();
        }

        private static readonly List<CustomRole> RegisteredRoles = new List<CustomRole>();

        private static readonly List<CustomAbility> RegisteredAbilities = new List<CustomAbility>();

        private Harmony harmony;

        private void CustomEvents_LoadedPlugins() => this.Register();

        private void Register()
        {
            var toRegisterRoles = Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass).Where(x => x.GetInterface(nameof(IMistakenCustomRole)) != null);
            RegisteredRoles.AddRange(Extensions.RegisterRoles(toRegisterRoles));
            foreach (var role in RegisteredRoles)
                Log.Debug($"Successfully registered {role.Name} ({role.Id})", this.Config.VerbouseOutput);

            if (RegisteredRoles.Count < toRegisterRoles.Count())
                Log.Warn($"Successfully registered {RegisteredRoles.Count}/{toRegisterRoles.Count()} CustomRoles!");

            var toRegisterAbilities = Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass).Where(x => x.IsSubclassOf(typeof(CustomAbility)));
            RegisteredAbilities.AddRange(Extensions.RegisterAbilities(toRegisterAbilities));
            foreach (var ability in RegisteredAbilities)
                Log.Debug($"Successfully registered {ability.Name} ({ability.AbilityType})", this.Config.VerbouseOutput);

            if (RegisteredAbilities.Count < toRegisterAbilities.Count())
                Log.Warn($"Successfully registered {RegisteredAbilities.Count}/{toRegisterAbilities.Count()} CustomAbilities!");
        }

        private void UnRegister()
        {
            short unregisteredRolesCount = 0;
            foreach (var role in CustomRole.UnregisterRoles(RegisteredRoles))
            {
                Log.Debug($"Successfully unregistered {role.Name} ({role.Id})", this.Config.VerbouseOutput);
                RegisteredRoles.Remove(role);
                unregisteredRolesCount++;
            }

            if (RegisteredRoles.Count > 0)
                Log.Warn($"Successfully unregistered {RegisteredRoles.Count}/{unregisteredRolesCount} CustomRoles!");

            short unregisteredAbilitiesCount = 0;
            foreach (var ability in CustomAbility.UnregisterAbilities(RegisteredAbilities))
            {
                Log.Debug($"Successfully unregistered {ability.Name} ({ability.AbilityType})", this.Config.VerbouseOutput);
                RegisteredAbilities.Remove(ability);
                unregisteredAbilitiesCount++;
            }

            if (RegisteredRoles.Count > 0)
                Log.Warn($"Successfully unregistered {RegisteredAbilities.Count}/{unregisteredAbilitiesCount} CustomAbilities!");
        }
    }
}
