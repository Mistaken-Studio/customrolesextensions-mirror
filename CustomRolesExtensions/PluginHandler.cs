// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.CustomRoles.API.Features;

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
        public override Version RequiredExiledVersion => new Version(3, 7, 2);

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            base.OnEnabled();
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins += this.CustomEvents_LoadedPlugins;
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            base.OnDisabled();
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins -= this.CustomEvents_LoadedPlugins;
            this.UnRegister();
        }

        private static readonly List<CustomRole> Registered = new List<CustomRole>();

        private void CustomEvents_LoadedPlugins() => this.Register();

        private void Register()
        {
            foreach (var type in Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass))
            {
                if (type.GetInterfaces().Any(x => x == typeof(IMistakenCustomRole)))
                {
                    var role = Activator.CreateInstance(type, true) as CustomRole;
                    if (role.TryRegister())
                    {
                        Log.Debug($"Successfully registered {role.Name} ({role.Id})", this.Config.VerbouseOutput);
                        Registered.Add(role);
                    }
                    else
                        Log.Warn($"Failed to register {role.Name} ({role.Id})");
                }
            }
        }

        private void UnRegister()
        {
            foreach (var role in Registered.ToArray())
            {
                if (role.TryUnregister())
                {
                    Log.Debug($"Successfully unregistered {role.Name} ({role.Id})", this.Config.VerbouseOutput);
                    Registered.Remove(role);
                }
                else
                    Log.Warn($"Failed to unregister {role.Name} ({role.Id})");
            }
        }
    }
}
