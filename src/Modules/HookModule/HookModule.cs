using HarmonyLib;
using System.Reflection;

namespace CementTools.Modules.HookModule
{
    /// <summary>
    /// This is a really simple hooking library that uses Harmony, basically modular UltiLib.
    /// See <see cref="CreateHook(CementHook)"/> for usage.
    /// </summary>
    public static class HookModule
    {
        /// <summary>
        /// A struct containing information to pass to <see cref="CreateHook"/>.
        /// </summary>
        public struct CementHook
        {
            public MethodInfo original;
            public CementMod callingMod;
            public MethodInfo hook;
            public bool isPrefix;

            public CementHook(CementMod mod, MethodInfo original, MethodInfo hook, bool isPrefix) : this()
            {
                this.original = original;
                callingMod = mod;
                this.hook = hook;
                this.isPrefix = isPrefix;
            }
        }

        /// <summary>
        /// Create a hook on a method that will toggle on and off with the passed CementMod.
        /// </summary>
        /// <param name="hook">The <see cref="CementHook"/> info to patch with. Just instantiate with the <c>new</c> keyword.</param>
        public static void CreateHook(CementHook hook)
        {
            HarmonyLib.Harmony modHarmony = new("hookmodulegen_" + hook.callingMod.name);

            // Ignores the hook if the callingMod is disabled.
            Func<bool> doBeforeHook = () =>
            {
                if (hook.callingMod.modFile.GetBool("Disabled")) return false;
                return true;
            };

            HarmonyMethod prefix = hook.isPrefix ? new HarmonyMethod(hook.hook) : null;
            HarmonyMethod postfix = hook.isPrefix ? null : new HarmonyMethod(hook.hook);

            HarmonyMethod beforeEitherFix = new HarmonyMethod(doBeforeHook.Method);
            
            modHarmony.Patch(hook.original, prefix, postfix);
            modHarmony.Patch(hook.hook, beforeEitherFix);

            Cement.Log($"New {(hook.isPrefix ? "PREFIX" : "POSTFIX")} hook on {hook.original.DeclaringType.Name}.{hook.original.Name} to {hook.hook.DeclaringType.Name}.{hook.hook.Name}");
        }
    }
}
