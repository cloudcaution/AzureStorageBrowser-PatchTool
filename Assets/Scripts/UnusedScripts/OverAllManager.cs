using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OverAllManager : MonoBehaviour
{
    public GameObject rightClick;
    public bool showRightClickMenu;
    private Button button;
    public bool hover;

    private void Awake()
    {
        button = rightClick.GetComponentInChildren<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (showRightClickMenu)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (!hover)
                    SetEnable(false); // StartCoroutine("Delay", 0.05f);
            }
            return;
        }

        if (rightClick.activeSelf)
        {
            SetEnable(true);
        }
    }

    private void SetEnable(bool active)
    {
        showRightClickMenu = active;
        rightClick.SetActive(active);
    }

    IEnumerator Delay(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        SetEnable(false);
    }
}
