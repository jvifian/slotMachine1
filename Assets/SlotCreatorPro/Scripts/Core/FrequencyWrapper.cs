using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class FrequencyWrapper
{
	public List<int> freq;

	public FrequencyWrapper(int numberOfReels) {
		freq = new List<int>();
		freq.Capacity = numberOfReels;
		for (int reel = 0; reel < numberOfReels; reel++)
			freq.Add (1);
	}
}