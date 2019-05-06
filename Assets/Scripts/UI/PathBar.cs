using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PathBar : MonoBehaviour
{
    public SelectedFileInfo info;
    private Text text;
    private string container => "/" + info.Container + "/";
    private string path => info.AbsolutePath;

    private void OnEnable()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        text.text = "root" + container + path;
    }
}
