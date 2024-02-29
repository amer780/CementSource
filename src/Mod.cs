using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CementTools
{
    public class Mod : BasePlugin
    {
        internal static ManualLogSource CMTLogger { get; private set; }

        public override void Load()
        {
            Harmony.CreateAndPatchAll(GetType());
            AddComponent<Cement>();
            CMTLogger = Log;
        }
    }
}
