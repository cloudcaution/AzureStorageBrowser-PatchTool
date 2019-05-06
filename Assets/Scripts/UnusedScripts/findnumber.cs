using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class findnumber : MonoBehaviour
{
    // Start is called before the first frame update
    public string input;
    void Start()
    {
        Debug.Log(findnumb(input));
    }

    string tmp = "";
    int startpoint, endpoint = 0;
    bool find = false, positive = true;

    int findnumb(string a)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (!find)
            {
                if (isLegal(a, i))
                {
                    find = true;
                    startpoint = endpoint = i;
                    if (char.IsDigit(a[i]))
                        tmp += a[i];
                    else
                        if (a[i] == '-')
                            positive = false;
                }
            }
            else
            {
                if (isLegal(a, i))
                {
                    tmp += a[i];
                }
                else
                    break;
            }
        }

        if (tmp == "" || (tmp.Length == 1 && char.IsDigit(tmp[0])))
            return 0;


        return ConverToNum(tmp);
    }
    
    bool isLegal(string a, int index)
    {
        if (!char.IsDigit(a[index]) && (a[index] != '+' && a[index] != '-'))
            return false;
        return true;    
    }

    int ConverToNum(string a)
    {
        int tmp = 0;
        for (int i = 0; i < a.Length; i++)
        {
            tmp += (a[i] & 15) * (int)Mathf.Pow(10, a.Length - 1 - i); 
        }

        if (positive)
            return tmp;
        else
            return 0 - tmp;
    }
}
