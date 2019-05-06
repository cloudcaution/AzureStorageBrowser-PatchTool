using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeList : MonoBehaviour
{
    ListNode list1 = new ListNode(2);
    ListNode list2 = new ListNode(1);

    private void Awake()
    {
        list1 = AddNode(list1, new int[] { 1, 5, 5 });
        list2 = AddNode(list2, new int[] { 5, 5, 5 });
        ListNodePrinter(list1);
        ListNodePrinter(list2);
        ListNodePrinter(AddTwoNumbers(list1, list2));
    }

    void ListNodePrinter(ListNode lista)
    {
        ListNode list = lista;
        string tmp = "";
        while (list != null)
        {
            tmp += list.val + " ";
            list = list.next;
        }
        Debug.Log(tmp);
    }

    private ListNode AddNode(ListNode list, int[] array)
    {
        ListNode lista = list;
        for (int i = 0; i < array.Length; i++)
        {
            lista.next = new ListNode(array[i]); ;
            lista = lista.next;    
        }
        return list;
    }

    public ListNode AddTwoNumbers(ListNode l1, ListNode l2)
    {
        ListNode l3 = new ListNode(0);
        ListNode l4;
        l4 = l3;
        int sum = 0;
        bool next = false;

        while (l1 != null || l2 != null)
        {
            if (l1 != null)
            {
                sum += l1.val;
                l1 = l1.next;
            }

            if (l2 != null)
            {
                sum += l2.val;
                l2 = l2.next;
            }
            if (next)
            {
                sum += 1;
                next = false;
            }

            if (sum >= 10)
            {
                sum = sum - 10;
                next = true;
            }
            //Debug.Log(sum + " " + (l1 != null).ToString() + " " + (l2 != null).ToString());
            l3.val = l3.val + sum;
            if (l1 == null && l2 == null && !next)
            {
                break;
            }
            l3.next = new ListNode(0);
            l3 = l3.next;
            sum = 0;
            if (l1 == null && l2 == null)
            {
                if (next)
                {
                    l3.val = 1;
                }
            }
        }
        return l4;
    }
}


public class ListNode
{
    public int val;
    public ListNode next;
    public ListNode(int x) { val = x; }
}
