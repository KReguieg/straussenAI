using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

[System.Serializable]
public class StateActivationEntry
{
    public Object target;

    public ActionType action;

    public MonoBehaviour targetScript;
    public string methodName;

    public string parameterValue;

    public float delayInSeconds = 0f;


    public enum ActionType
    {
        ActivateGameObject,
        DeactivateGameObject,
        EnableComponent,
        DisableComponent,
        InvokeMethod
    }

    public async Task Execute(MonoBehaviour host)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        if (delayInSeconds > 0f)
        {
            host.StartCoroutine(DelayedExecute(tcs));
        }
        else
        {
            ExecuteNow();
            tcs.SetResult(true);
        }

        await tcs.Task;
    }

    private IEnumerator DelayedExecute(TaskCompletionSource<bool> tcs)
    {
        yield return new WaitForSeconds(delayInSeconds);
        ExecuteNow();
        tcs.SetResult(true);
    }

    private void ExecuteNow()
    {
        switch (action)
        {
            case ActionType.ActivateGameObject:
                if (target is GameObject go1) go1.SetActive(true);
                break;
            case ActionType.DeactivateGameObject:
                if (target is GameObject go2) go2.SetActive(false);
                break;
            case ActionType.EnableComponent:
                if (target is Behaviour b1) b1.enabled = true;
                break;
            case ActionType.DisableComponent:
                if (target is Behaviour b2) b2.enabled = false;
                break;
            case ActionType.InvokeMethod:
                if (targetScript != null && !string.IsNullOrEmpty(methodName))
                {
                    MethodInfo method = targetScript.GetType().GetMethod(methodName,
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic);

                    if (method != null)
                    {
                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length == 0)
                            method.Invoke(targetScript, null);
                        else if (parameters.Length == 1)
                        {
                            object parsedParameter = ConvertParameter(parameterValue, parameters[0].ParameterType);
                            method.Invoke(targetScript, new object[] { parsedParameter });
                        }

                    }
                    else
                    {
                        Debug.LogWarning($"[StateActivationEntry] Method '{methodName}' not found on {targetScript.name}");
                    }
                }
                break;

        }
    }

    private IEnumerable<string> GetMethodNames()
    {
        if (targetScript == null)
            return new List<string>();

        return targetScript.GetType()
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
            .Where(m => /*m.GetParameters().Length == 0 &&*/ m.ReturnType == typeof(void))
            .Select(m => m.Name);
    }

    private object ConvertParameter(string value, System.Type targetType)
    {
        try
        {
            if (targetType == typeof(int)) return int.Parse(value);
            if (targetType == typeof(float)) return float.Parse(value);
            if (targetType == typeof(bool)) return bool.Parse(value);
            if (targetType == typeof(string)) return value;
            if (targetType == typeof(GameObject)) return GameObject.Find(value);

            Debug.LogWarning($"[StateActivationEntry] Unsupported parameter type: {targetType.Name}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StateActivationEntry] Failed to convert parameter: {ex.Message}");
        }

        return null;
    }




}
