using MelonLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CementTools
{
    public class Mod : MelonMod
    {
        private GameObject _cementContainer;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            _cementContainer = new GameObject("CementContainer");
            UnityEngine.Object.DontDestroyOnLoad(_cementContainer);
            _cementContainer.AddComponent<Cement>();
        }
    }
}
