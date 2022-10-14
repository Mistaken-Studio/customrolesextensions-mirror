// -----------------------------------------------------------------------
// <copyright file="PluginHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomRoles.API.Features;
using HarmonyLib;
using Mistaken.Updater.API.Config;

namespace Mistaken.API.CustomRoles
{
    internal sealed class PluginHandler : Plugin<Config>, IAutoUpdateablePlugin
    {
        public override string Author => "Mistaken Devs";

        public override string Name => "CustomRolesExtensions";

        public override string Prefix => "MCRolesExtensions";

        public override PluginPriority Priority => PluginPriority.Default + 1;

        public override Version RequiredExiledVersion => new(5, 2, 2);

        public AutoUpdateConfig AutoUpdateConfig => new()
        {
            Type = SourceType.GITLAB,
            Url = "https://git.mistaken.pl/api/v4/projects/69",
        };

        public override void OnEnabled()
        {
            _harmony.PatchAll();

            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins += this.Register;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            _harmony.UnpatchAll();

            this.UnRegister();
            Mistaken.Events.Handlers.CustomEvents.LoadedPlugins -= this.Register;

            base.OnDisabled();
        }

        private static readonly List<CustomRole> _registeredRoles = new();

        private static readonly List<CustomAbility> _registeredAbilities = new();

        private static readonly Harmony _harmony = new("com.customrolesextensions.patch");

        private void Register()
        {
            _registeredAbilities.AddRange(this.RegisterAbilities());
            foreach (var ability in _registeredAbilities)
                Log.Debug($"Successfully registered {ability.Name} ({ability.AbilityType})", this.Config.VerboseOutput);

            _registeredRoles.AddRange(this.RegisterRoles());
            foreach (var role in _registeredRoles)
                Log.Debug($"Successfully registered {role.Name} ({role.Id})", this.Config.VerboseOutput);
        }

        private void UnRegister()
        {
            foreach (var ability in CustomAbility.UnregisterAbilities(_registeredAbilities))
            {
                Log.Debug($"Successfully unregistered {ability.Name} ({ability.AbilityType})", this.Config.VerboseOutput);
                _registeredAbilities.Remove(ability);
            }

            foreach (var role in CustomRole.UnregisterRoles(_registeredRoles))
            {
                Log.Debug($"Successfully unregistered {role.Name} ({role.Id})", this.Config.VerboseOutput);
                _registeredRoles.Remove(role);
            }
        }

        private IEnumerable<CustomRole> RegisterRoles()
        {
            List<CustomRole> registeredRoles = new();
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
                        try
                        {
                            customRole.GetType().GetMethod("TryRegister", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(customRole, null);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("CustomRole");
                            Log.Error(ex);
                        }

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

        private IEnumerable<CustomAbility> RegisterAbilities()
        {
            List<CustomAbility> registeredAbilities = new();
            foreach (Type type in Exiled.Loader.Loader.Plugins.Where(x => x.Config.IsEnabled).SelectMany(x => x.Assembly.GetTypes()).Where(x => !x.IsAbstract && x.IsClass).Where(x => x.IsSubclassOf(typeof(CustomAbility))))
            {
                if (!type.IsSubclassOf(typeof(CustomAbility)) || type.GetCustomAttribute(typeof(CustomAbilityAttribute)) is null)
                    continue;

                foreach (Attribute attribute in type.GetCustomAttributes(typeof(CustomAbilityAttribute), true))
                {
                    try
                    {
                        CustomAbility customAbility = (CustomAbility)Activator.CreateInstance(type);
                        try
                        {
                            customAbility.GetType().GetMethod("TryRegister", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(customAbility, null);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("CustomAbility");
                            Log.Error(ex);
                        }

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
