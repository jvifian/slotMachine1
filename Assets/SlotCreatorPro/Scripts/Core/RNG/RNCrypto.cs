using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System;

public class RNCrypto : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static int GetRandomIntBetween(int minValue, int maxValue)
	{
		// Make maxValue inclusive.
		//maxValue++;
		
		var rng = new RNGCryptoServiceProvider();
		var uint32Buffer = new byte[4];
		long diff = maxValue - minValue;
		
		while (true)
		{
			rng.GetBytes(uint32Buffer);
			uint rand = BitConverter.ToUInt32(uint32Buffer, 0);
			const long max = (1 + (long)int.MaxValue);
			long remainder = max % diff;
			if (rand < max - remainder)
			{
				return (int)(minValue + (rand % diff));
			}
		}
	}
}
