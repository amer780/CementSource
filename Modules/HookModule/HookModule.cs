using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using CementTools;

namespace HookModule
{
    /// <summary>
    /// This is a really simple hooking library that uses Harmony, basically modular UltiLib.
    /// Mods should use this for private getters, setters, and methods that can't be hooked with BepInEx.MonoMod.HookGenPatcher
    /// or if the mod's hook needs to be disabled once the mod is toggled off in-game.
    /// </summary>
    public static class HookModule
    {
        /// <summary>
        /// A struct containing information required to create toggleable Harmony "hooks", or patches, with Cement.
        /// </summary>
        public struct CementHook
        {
            public MethodInfo original;
            public CementMod callingMod;
            public MethodInfo hook;
            public bool isPrefix;

            public CementHook(MethodInfo original, MethodInfo hook, bool isPrefix) : this()
            {
                this.original = original;
                this.hook = hook;
                this.isPrefix = isPrefix;
            }
        }
        private static Harmony defaultHarmony;

        /// <summary>
        /// Create a hook on a method that will toggle on and off with the passed CementMod.
        /// </summary>
        /// <param name="hook">The <see cref="CementHook"/> info to patch with.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CreateHook(CementHook hook)
        {
            Harmony modHarmony = new Harmony(hook.callingMod.name);

            HarmonyMethod prefix = hook.isPrefix ? new HarmonyMethod(hook.hook) : null;
            HarmonyMethod postfix = hook.isPrefix ? null : new HarmonyMethod(hook.hook);
            modHarmony.Patch(hook.original, prefix, postfix);
            //TODO: On mod disabled, remove the hook

            Cement.Log($"New {(hook.isPrefix ? "PREFIX" : "POSTFIX")} hook on {hook.original.DeclaringType.Name}.{hook.original.Name} to {hook.hook.DeclaringType.Name}.{hook.hook.Name}");
        }

        public static void RemoveHook(CementHook hook)
        {
            defaultHarmony.Unpatch(hook.original, hook.hook);
        }
    }
}
