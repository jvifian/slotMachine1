using UnityEngine;
using System.Collections;

public enum pRNGs {
	RngNativeDotNet,
	RngMersenneTwister,
	RngCryptoServiceProvider,
}

public class RNGManager : MonoBehaviour {

	public static MersenneTwister rng = new MersenneTwister();

	public static uint getRandomRange(pRNGs activeRNG, int min, int max)
	{
		uint result = 0;
		switch (activeRNG)
		{
		case pRNGs.RngNativeDotNet:
			result = (uint)UnityEngine.Random.Range(min, max);
			break;
		case pRNGs.RngMersenneTwister:
			result = rng.NextUInt32((uint)min, (uint)max);
			break;
		case pRNGs.RngCryptoServiceProvider:
			//#if !UNITY_IPHONE && !UNITY_ANDROID
			result = (uint)RNCrypto.GetRandomIntBetween(min, max);
			//#endif
			break;
		}
		return result;
	}
}
