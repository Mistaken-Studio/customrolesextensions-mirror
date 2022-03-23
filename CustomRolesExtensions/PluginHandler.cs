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

        private static readonly List<CustomRole> Registered = new List<CustomRole>();

        private Harmony harmony;

        private void CustomEvents_LoadedPlugins() => this.Register();

        private void Register()
        {
            var toRegister = Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass).Where(x => x.GetInterface(nameof(IMistakenCustomRole)) != null);
            Registered.AddRange(Extensions.RegisterRoles(toRegister));
            foreach (var role in Registered)
                Log.Debug($"Successfully registered {role.Name} ({role.Id})", this.Config.VerbouseOutput);

            if (Registered.Count < toRegister.Count())
                Log.Warn($"Successfully registered {Registered.Count}/{toRegister.Count()} CustomRoles!");
        }

        private void UnRegister()
        {
            short unregisteredCount = 0;
            foreach (var role in CustomRole.UnregisterRoles(Registered))
            {
                Log.Debug($"Successfully unregistered {role.Name} ({role.Id})", this.Config.VerbouseOutput);
                Registered.Remove(role);
                unregisteredCount++;
            }

            if (Registered.Count > 0)
                Log.Warn($"Successfully unregistered {Registered.Count}/{unregisteredCount} CustomRoles!");
        }
    }
}
