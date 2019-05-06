using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FindPath : MonoBehaviour {

    public string name;
    public Text test;

	// Use this for initialization
	void Start () {
        test.text = GetApplicationPath(name);
	}

    public string GetApplicationPath(string appname)
    {
        using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.RegistryKey.OpenRemoteBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, ""))
        {
            using (Microsoft.Win32.RegistryKey subkey = key.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\" + appname))
            {
                if (subkey == null)
                    return "fake";

                object path = subkey.GetValue("Path");

                if (path != null)
                    return (string)path;
            }

        }
        return "wrong";
    }
}
