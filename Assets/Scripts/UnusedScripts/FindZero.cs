using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class FindZero : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MatchCollection matchCollection = regex.Matches("adad dwad"); 
        foreach(Match match in matchCollection)
        {
            Debug.Log(match.Value);
        }
    }

    Regex regex = new Regex(@"[a-z]{2,3}");


    Stack<int> stack = new Stack<int>();
    Queue<int> queue = new Queue<int>();
}
