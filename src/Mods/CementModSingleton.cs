using System;
using System.Collections.Generic;

namespace CementTools
{
    public static class CementModSingleton
    {
        private static readonly Dictionary<string, CementMod> _singletons = new Dictionary<string, CementMod>();
        private static readonly List<CementMod> cementMods = new List<CementMod>();

        public static void Add(Type t, CementMod i)
        {
            _singletons[t.Name] = i;
            cementMods.Add(i);
        }

        public static T Get<T>() where T : CementMod
        {
            return _singletons[typeof(T).Name] as T;
        }

        public static CementMod[] GetAll()
        {
            return cementMods.ToArray();
        }
    }
}