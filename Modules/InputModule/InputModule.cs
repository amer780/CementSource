using System.Collections.Generic;
using System;
using UnityEngine;
using Femur;
using UnityEngine.InputSystem;
using CementTools;
using System.Reflection;
using GB.Input;
using UnityEngine.InputSystem.Users;

namespace InputModule
{
    public class CallbackManager
    {
        public CallbackManager(string input)
        {
            inputToCheck = input;
        }

        public CallbackManager(string[] inputs)
        {
            inputsToCheck = inputs;
        }

        private string inputToCheck;
        private string[] inputsToCheck;

        private List<Action<Actor>> callbacks = new List<Action<Actor>>();

        public void bind(Action<Actor> callback)
        {
            callbacks.Add(callback);
        }

        public void unbind(Action<Actor> callback)
        {
            if (callbacks.Contains(callback))
            {
                callbacks.Remove(callback);
            }
        }

        public void TriggerCallbacks(Actor actor)
        {
            foreach (var callback in callbacks)
            {
                callback(actor);
            }
        }
    }

    public class InputManager : CementMod
    {
        private static Dictionary<string, CallbackManager> onKey = new Dictionary<string, CallbackManager>();
        private static Dictionary<string, CallbackManager> onKeyHeld = new Dictionary<string, CallbackManager>();
        private static Dictionary<string[], CallbackManager> onKeys = new Dictionary<string[], CallbackManager>();
        private static Dictionary<string[], CallbackManager> onKeysHeld = new Dictionary<string[], CallbackManager>();

        List<string> pressedLastFrame1 = new List<string>();
        List<string> pressedLastFrame2 = new List<string>();

        bool i = true;

        private InputDevice GetLastDetectedInputDevice()
        {
            return MonoSingleton<UnityInputSystemManager>.Instance.GetLastDetectedInputDevice();
        }

        private InputUser GetUserFromDevice(InputDevice device)
        {
            foreach (InputUser user in InputUser.all)
            {
                foreach (InputDevice d in user.pairedDevices)
                {
                    if (d == device)
                    {
                        return user;
                    }
                }
            }

            return default(InputUser);
        }

        public static InputDevice GetInputDeviceFromActor(Actor actor)
        {
            return actor.InputPlayer.pairedDevices[0];
        }

        public static CallbackManager onInput(string inputName)
        {
            if (!onKey.ContainsKey(inputName))
            {
                onKey[inputName] = new CallbackManager(inputName);
            }
            return onKey[inputName];
        }

        public static CallbackManager onInputs(params string[] inputNames)
        {
            if (!onKeys.ContainsKey(inputNames))
            {
                onKeys[inputNames] = new CallbackManager(inputNames);
            }
            return onKeys[inputNames];
        }

        public static CallbackManager onInputHeld(string inputName)
        {
            if (!onKeyHeld.ContainsKey(inputName))
            {
                onKeyHeld[inputName] = new CallbackManager(inputName);
            }
            return onKeyHeld[inputName];
        }

        public static CallbackManager onInputsHeld(params string[] inputNames)
        {
            if (!onKeysHeld.ContainsKey(inputNames))
            {
                onKeysHeld[inputNames] = new CallbackManager(inputNames);
            }
            return onKeysHeld[inputNames];
        }

        private bool wasPressedLastFrame(string inputPath)
        {
            if (i)
            {
                return pressedLastFrame1.Contains(inputPath);
            }
            return pressedLastFrame2.Contains(inputPath);
        }

        private int GetIndexOfActor(Actor actor)
        {
            for (int i = 0; i < Actor.RemoteActors.Count; i++)
            {
                if (actor == Actor.RemoteActors[i])
                {
                    return i;
                }
            }

            return -1;
        }

        public void Update()
        {
            if (i)
            {
                pressedLastFrame2.Clear();
            }
            else
            {
                pressedLastFrame1.Clear();
            }

            foreach (Actor actor in FindObjectsOfType<Actor>())
            {
                if (actor.ControlledBy == Actor.ControlledTypes.AI) continue;
                if (actor.ControlledBy == Actor.ControlledTypes.Animation)
                {
                    if (actor.InputPlayer.id == 0)
                    {
                        actor.InputPlayer = GetUserFromDevice(GetLastDetectedInputDevice());
                    }
                }
                InputDevice inputDevice = actor.InputPlayer.pairedDevices[0];

                foreach (string key in onKeyHeld.Keys)
                {
                    InputControl control = inputDevice.TryGetChildControl(key);
                    if (control != null)
                    {
                        if (control.IsPressed())
                        {
                            onKeyHeld[key].TriggerCallbacks(actor);
                        }
                    }
                }

                foreach (string key in onKey.Keys)
                {
                    InputControl control = inputDevice.TryGetChildControl(key);
                    if (control != null)
                    {
                        string hashCode = control.GetHashCode().ToString();

                        if (control.IsPressed())
                        {
                            Cement.Log(control);

                            if (!wasPressedLastFrame(hashCode))
                            {
                                onKey[key].TriggerCallbacks(actor);
                            }

                            if (i)
                            {
                                pressedLastFrame2.Add(hashCode);
                            }
                            else
                            {
                                pressedLastFrame1.Add(hashCode);
                            }
                        }
                    }
                }

                foreach (string[] inputs in onKeysHeld.Keys)
                {
                    bool allPressed = true;
                    foreach (string input in inputs)
                    {
                        InputControl control = inputDevice.TryGetChildControl(input);
                        if (control != null)
                        {
                            if (!control.IsPressed())
                            {
                                allPressed = false;
                                break;
                            }
                        }
                        else
                        {
                            allPressed = false;
                            break;
                        }
                    }

                    if (allPressed)
                    {
                        onKeysHeld[inputs].TriggerCallbacks(actor);
                    }
                }

                foreach (string[] inputs in onKeys.Keys)
                {
                    string inputsCode = "";

                    bool allPressed = true;
                    foreach (string input in inputs)
                    {
                        InputControl control = inputDevice.TryGetChildControl(input);
                        if (control != null)
                        {
                            int hashCode = control.GetHashCode();
                            inputsCode += $"{hashCode}+";

                            if (!control.IsPressed())
                            {
                                allPressed = false;
                                break;
                            }
                        }
                        else
                        {
                            allPressed = false;
                            break;
                        }
                    }

                    if (allPressed)
                    {
                        if (!wasPressedLastFrame(inputsCode))
                        {
                            onKeys[inputs].TriggerCallbacks(actor);
                        }

                        if (i)
                        {
                            pressedLastFrame2.Add(inputsCode);
                        }
                        else
                        {
                            pressedLastFrame1.Add(inputsCode);
                        }
                    }
                }
            }

            i = !i;
        }
    }
}