using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class StateManager <T>
{
    Dictionary<T, MethodInfo> stateEnterMethods;
    Dictionary<T, MethodInfo> stateUpdateMethods;
    Dictionary<T, MethodInfo> stateExitMethods;

    T currentState;
    T previousState;

    object contextObject;
    bool verbose;

    float stateStartTime;


    //  #METHODS
    public void RegisterStateMethods (object contextObject, bool verbose)
    {
        this.contextObject = contextObject;
        this.verbose = verbose;

        stateEnterMethods = new Dictionary<T, MethodInfo>();
        stateUpdateMethods = new Dictionary<T, MethodInfo>();
        stateExitMethods = new Dictionary<T, MethodInfo>();

        string log = "RegisterStateMethods() found the following state methods:  \n";

        foreach (var enumValue in System.Enum.GetValues(typeof(T)))
        {
            log += "GameState." + enumValue.ToString() + ": ";

            System.Type contextObjectType = contextObject.GetType();

            MethodInfo enterMethod = contextObjectType.GetMethod(enumValue.ToString() + "Enter", BindingFlags.NonPublic | BindingFlags.Instance);
            if (enterMethod != null)
            {
                stateEnterMethods.Add((T)enumValue, enterMethod);
                log += "Enter ";
            }

            MethodInfo updateMethod = contextObjectType.GetMethod(enumValue.ToString() + "Update", BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateMethod != null)
            {
                stateUpdateMethods.Add((T)enumValue, updateMethod);
                log += "Update ";
            }

            MethodInfo exitMethod = contextObjectType.GetMethod(enumValue.ToString() + "Exit", BindingFlags.NonPublic | BindingFlags.Instance);
            if (exitMethod != null)
            {
                stateExitMethods.Add((T)enumValue, exitMethod);
                log += "Exit ";
            }

            log += "\n";
        }

        if (verbose)
        {
            Debug.Log(log);
        }
    }

    public void SwitchState(T newState)
    {
        if (verbose)
        {
            Debug.LogFormat("Switching state from {0} to {1}", currentState, newState);
        }

        if (stateExitMethods.ContainsKey(currentState))
        {
            stateExitMethods[currentState].Invoke(contextObject, null);
        }

        previousState = currentState;
        currentState = newState;

        stateStartTime = Time.unscaledTime;

        if (stateEnterMethods.ContainsKey(currentState))
        {
            stateEnterMethods[currentState].Invoke(contextObject, null);
        }
    }

    public void UpdateState()
    {
        if (stateUpdateMethods.ContainsKey(currentState))
        {
            stateUpdateMethods[currentState].Invoke(contextObject, null);
        }
    }


    public T GetState()
    {
        return currentState;
    }
    public T GetPreviousState()
    {
        return previousState;
    }
    public float GetStateStartTime()
    {
        return stateStartTime;
    }
}

