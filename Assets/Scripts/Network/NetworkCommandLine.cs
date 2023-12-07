using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager netManager;
    // Start is called before the first frame update
    void Start()
    {
        netManager = GetComponent<NetworkManager>();

        if (Application.isEditor) return;

        var args = GetCommandlineArgs();
    }

    private Dictionary<string, string> GetCommandlineArgs()
    {
        Dictionary<string,string> argDictionary = new Dictionary<string,string>();

        var args = System.Environment.GetCommandLineArgs();

        for(int i = 0; i < args.Length; ++i)
        {
            var arg = args[i].ToLower();
            if(arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[i + 1].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;
                argDictionary.Add(arg, value);
            }
        }
        return argDictionary;
    }
}
