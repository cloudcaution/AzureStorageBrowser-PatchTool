using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class NonRepeatingSubstring : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(LengthOfLongestSubstring("dwadlkawldjaklwj"));
        Debug.Log(Convert("dawdawdawd", 4));
    }

    public int LengthOfLongestSubstring(string s)
    {
        int n = s.Length;
        HashSet<char> set = new HashSet<char>();
        int ans = 0, i = 0, j = 0;
        while (i < n && j < n)
        {
            // try to extend the range [i, j]
            if (!set.Contains(s[j]))
            {
                set.Add(s[j++]);
                ans = Math.Max(ans, j - i);
            }
            else
            {
                set.Remove(s[i++]);
            }
        }
        return ans;
    }

    public string Convert(string s, int numRows)
    {
        int counter = 0;
        List<char> tmp = new List<char>();
        for (int i = 0; i < numRows; i++)
        {
            counter = 0;
            for (int k = 0; k + i < s.Length; k += 2 * (numRows - 1) * counter)
            {
                tmp.Add(s[k + i]);
                counter++;
            }
        }

        string b = "";
        foreach (char a in tmp)
        {
            b += a;
        }
        return b.ToString();
    }

    public void Diction()
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();
        foreach(KeyValuePair<string, string> a in dict)
        {
            dict[a.Key] = "dwadw";
        }
        RectTransformUtility.WorldToScreenPoint(Camera.current, new Vector3(1f, 2f, 3f));
    }

    public string solution(string s, int numRows)
    {
        if (numRows == 1) return s;

        List<StringBuilder> rows = new List<StringBuilder>();
        for (int i = 0; i < Math.Min(numRows, s.Length); i++)
            rows.Add(new StringBuilder());

        int curRow = 0;
        bool goingDown = false;

        foreach (char c in s)
        {
            rows[curRow].Append(c);
            if (curRow == 0 || curRow == numRows - 1) goingDown = !goingDown;
            curRow += goingDown ? 1 : -1;
        }

        StringBuilder ret = new StringBuilder();
        foreach (StringBuilder row in rows) ret.Append(row);
        return ret.ToString();
    }
}
