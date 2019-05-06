using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DelegateTest : MonoBehaviour {

    public delegate void TicketSeller(int price);
    public TicketSeller Seller;
    public Text text;

	// Use this for initialization
	void Start () {
        Task.Run(() => Stay());
	}

    private void MovieTicket(int price)
    {
        Debug.Log(price);
    }

    private void TheatreTicket(int price)
    {
        Debug.Log(price * 2);
    }

    private void Stay()
    {
        text.text = "dada";
        Counter c = new Counter();
        c.ThresholdReached += c_ThresholdReached;
        c.OnThresholdReached(new ThresholdReachedEventArgs { Threshold = 0, TimeReached = DateTime.Now });
    }

    void c_ThresholdReached(object sender, EventArgs e)
    {
        Debug.Log("The threshold was reached.");
    }

}

public class Counter
{
    public event EventHandler ThresholdReached;

    public virtual void OnThresholdReached(EventArgs e)
    {
        ThresholdReached?.Invoke(this, e);
    }

    // provide remaining implementation for the class
}

public class ThresholdReachedEventArgs : EventArgs
{
    public int Threshold { get; set; }
    public DateTime TimeReached { get; set; }
}
