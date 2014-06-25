// Brad Lima - Bulkhead Studios 2014

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using System;

[RequireComponent(typeof(SlotCredits))]
[RequireComponent(typeof(SlotCompute))]
[RequireComponent(typeof(SlotComputeEditor))]
[RequireComponent(typeof(SlotWins))]
[RequireComponent(typeof(SlotLines))]
public class Slot : MonoBehaviour {

	#region Actions
	public static event Action<SlotState> OnSlotStateChangeTo;
	public static event Action<SlotState> OnSlotStateChangeFrom;
	public static event Action<SlotState> OnSlotGUI;

	public static event Action OnSlotUpdate;

	public static event Action<SlotWinData> OnSpinBegin;
	public static event Action OnSpinInsufficentCredits;

	public static event Action<int> OnReelLand;
	public static event Action OnSpinSnap;
	public static event Action<int, int>OnSpinDone;
	public static event Action OnSpinDoneNoWins;

	public static event Action<int>OnBeginCreditWinCountOff;
	public static event Action<int>OnBeginCreditBonusCountOff;
	public static event Action<int>OnCompletedCreditCountOff;
	public static event Action OnCompletedBonusCreditCountOff;

	public static event Action<SlotWinData> OnLineWinComputed;
	public static event Action<SlotWinData, bool> OnLineWinDisplayed;
	public static event Action<SlotWinData> OnBeginDelayBetweenLineWinDisplayed;
	public static event Action<int, int> OnAllWinsComputed;
	public static event Action<SlotScatterHitData>OnScatterSymbolHit;
	public static event Action<GameObject, int>OnScatterSymbolLanded;
	public static event Action<SlotScatterHitData>OnAnticipationScatterBegin;

	public static event Action<int> OnWinDisplayedCycle;

	public static event Action<int> OnIncrementLinesPlayed;
	public static event Action<int> OnIncrementBetPerLine;

	public static event Action<GameObject> OnSymbolReturningToPool;

	#endregion

	public bool debugMode = true;

	public pRNGs activeRNG;
	public string callbackScriptName;

	public int reelHeight = 5;				// how many symbols tall are the reels to be on this slot machine
	public int reelIndent = 1;
	public int numberOfReels = 3;
	public float spinTime = 1.0f;			// how long should the total spin time be?
	public float spinTimeIncPerReel = 0.5f;	// how much longer should each extra reel spin (spinTime + spinTimeIncPerReel)
	public float spinTimeScatterAnticipation = 0.5f;

	public float spinningSpeed = 0.033f;
	public float easeOutTime = 0.5f; 
	public EaseType reelEase = EaseType.EaseOutElastic;				// what type of HOTween Ease will be applied when the reel stops

	public bool usePool = true;

	public GameObject reelBackground;
	 
	//public List<int> betsPerLine;


	public List<GameObject> symbolPrefabs;	// A list of symbol s (attach SlotSymbol script to each symbol)
	public List<GameObject> winboxPrefabs;	// A list of symbol prefabs (attach SlotSymbol script to each symbol)
	public List<FrequencyWrapper> reelFrequencies;
	public List<int> symbolFrequencies;		// A list of frequencies of each symbol on this slot machine

	public List<string> symbolSetNames;		// A list of names describing each symbol set (i.e. Triple Sevens)
	[SerializeField]
	public SaveEditor edsave;
	[SerializeField]
	public List<BetsWrapper> betsPerLine;
	[SerializeField]
	public List<SetsWrapper> symbolSets;  	// A list of strings describing each valid symbol set (i.e. ,0,1,2,) must start and end with commas
	[SerializeField]
	public List<PaysWrapper> setPays;
	[SerializeField]
	public List<LinesWrapper> lines;		// a list of strings that describe each valid line on the slot machine using symbol indexes

	[HideInInspector]
	public SlotState state = SlotState.ready;
	[HideInInspector]
	public Dictionary<int, SlotReel> reels = new Dictionary<int, SlotReel>();
	[HideInInspector]
	public List<GameObject>[] pool;
	[HideInInspector]
	public List<GameObject>[] poolWinbox;
	[HideInInspector]
	public GameObject poolContainer;
	public GameObject poolWinboxContainer;

	public int percision;
	public SlotComponents refs;

	public int[,] suppliedResult;
	public bool useSuppliedResult;

	#region Startup
	void Awake () {

		if (usePool) createGlobalSymbolPool ();
		if (usePool) createGlobalWinboxPool ();

		createReelGameObjects();

		refs.credits = GetComponent<SlotCredits>();
		refs.wins = GetComponent<SlotWins>();
		refs.compute = GetComponent<SlotCompute>();
		refs.lines = GetComponent<SlotLines>();

		if (callbackScriptName != "")
		{
			gameObject.AddComponent(callbackScriptName);
		}

		if (betsPerLine.Count == 0)
		{
			betsPerLine.Add (new BetsWrapper());
			betsPerLine[0].value = 1;
			betsPerLine[0].canBet = true;
		}
	}
	void Start() {

	}
	void Update () {

		if (OnSlotUpdate != null)
			OnSlotUpdate();

	}

	void createReelGameObjects()
	{
		for (int reelNumber = 0; reelNumber < numberOfReels; reelNumber++)
		{
			GameObject reel = new GameObject("Reel" + (reelNumber + 1));
			reel.SetActive(false);
			reel.AddComponent(typeof(SlotReel));
			reel.GetComponent<SlotReel>().reelIndex = (reelNumber + 1);
			reel.transform.parent = this.transform;
			//reel.transform.localRotation = this.transform.rotation;
			reel.SetActive(true);
			reels[reelNumber] = reel.GetComponent<SlotReel>();
		}
	}
	
	void OnEnable()
	{
		SlotReel.OnReelDoneSpinning += OnReelDoneSpinning;
	}
	void OnDisable()
	{
		SlotReel.OnReelDoneSpinning -= OnReelDoneSpinning;
		
		releaseGlobalSymbolPool();
		releaseGlobalWinboxPool();
	}
	#endregion

	#region Create Pool
	void releaseGlobalSymbolPool()
	{
		// Release Symbol Pool
		if (usePool)
		{
			foreach(List<GameObject> list in pool)
			{
				foreach(GameObject obj in list)
					Destroy (obj);
				list.Clear ();
			}
			Destroy(poolContainer);
		}
	}

	void releaseGlobalWinboxPool()
	{
		// Release Symbol Pool
		if (usePool)
		{
			foreach(List<GameObject> list in poolWinbox)
			{
				foreach(GameObject obj in list)
					Destroy (obj);
				list.Clear ();
			}
			Destroy(poolWinboxContainer);
		}
	}

	void createGlobalSymbolPool()
	{
		poolContainer = new GameObject("_SymbolPool");
		poolContainer.transform.parent = transform;
		pool = new List<GameObject>[symbolPrefabs.Count];
		for(int prefabIndex = 0; prefabIndex < symbolPrefabs.Count; prefabIndex++)
		{
			pool[prefabIndex] = new List<GameObject>();
			for (int prefabCount = 0; prefabCount < (numberOfReels * reelHeight); prefabCount++)
			{
				if (symbolPrefabs[prefabIndex] == null) 
				{
					logConfigError(SlotErrors.MISSING_SYMBOL);
					return;
				}
				GameObject symb = (GameObject)Instantiate(symbolPrefabs[prefabIndex]);
				symb.GetComponent<SlotSymbol>().symbolIndex = prefabIndex;
				symb.SetActive(false);
				pool[prefabIndex].Add (symb);
				symb.transform.parent = poolContainer.transform;
				symb.transform.localScale = Vector3.Scale(symb.transform.localScale, transform.localScale);

				if (reelBackground != null)
				{
					GameObject reelbkg = (GameObject)Instantiate(reelBackground);
					reelbkg.transform.parent = symb.transform;
					reelbkg.transform.localScale = Vector3.Scale(reelbkg.transform.localScale, transform.localScale);
					//reelbkg.transform.localRotation = transform.rotation;
				}
			}
		}
		
	}

	void createGlobalWinboxPool()
	{
		poolWinboxContainer = new GameObject("_SymbolWinboxPool");
		poolWinboxContainer.transform.parent = transform;
		poolWinbox = new List<GameObject>[winboxPrefabs.Count];

		int numberOfPrefabsToPool = 0;
		foreach(PaysWrapper c in setPays)
		{
			if (c.pays.Count > numberOfPrefabsToPool)
				numberOfPrefabsToPool = c.pays.Count;
		}

		for(int prefabIndex = 0; prefabIndex < winboxPrefabs.Count; prefabIndex++)
		{
			poolWinbox[prefabIndex] = new List<GameObject>();
			//int numberOfPrefabsToPool = numberOfReels;
			//if (prefabIndex > symbolSets.Count - 1) continue;
			//if (symbolSets[prefabIndex].typeofSet == SetsType.scatter)
			//	numberOfPrefabsToPool = setPays[prefabIndex].pays.Count;
			for (int prefabCount = 0; prefabCount < numberOfPrefabsToPool; prefabCount++)
			{
				if (winboxPrefabs[prefabIndex] == null) 
				{
					continue;
					//logConfigError(SlotErrors.MISSING_SYMBOL);
					//return;
				}
				GameObject winb = (GameObject)Instantiate(winboxPrefabs[prefabIndex]);
				//winb.GetComponent<SlotSymbol>().symbolIndex = prefabIndex;
				winb.SetActive(false);
				poolWinbox[prefabIndex].Add (winb);
				winb.transform.parent = poolWinboxContainer.transform;
				winb.transform.localScale = Vector3.Scale(winb.transform.localScale, transform.localScale);
			}
		}
		
	}
	public GameObject getSymbolFromPool(int symbolIndex)
	{
		GameObject symbol = pool[symbolIndex][0];
		pool[symbolIndex].RemoveAt (0);
		symbol.transform.parent = null;
		symbol.SetActive(true);
		return symbol;
	}
	public void returnSymbolToPool(GameObject symbol)
	{
		if (OnSymbolReturningToPool != null)
			OnSymbolReturningToPool(symbol);

		pool[symbol.GetComponent<SlotSymbol>().symbolIndex].Add (symbol);
		symbol.transform.parent = poolContainer.transform;
		symbol.SetActive(false);
	}

	public GameObject getWinboxFromPool(int symbolIndex)
	{
		GameObject winbox = poolWinbox[symbolIndex][0];
		poolWinbox[symbolIndex].RemoveAt (0);
		winbox.transform.parent = null;
		winbox.SetActive(true);
		return winbox;
	}
	public void returnWinboxToPool(GameObject winbox, int symbolIndex)
	{
		//if (OnSymbolReturningToPool != null)
		//	OnSymbolReturningToPool(symbol);
		if (winbox.transform.parent.GetComponent<SlotSymbol>() != null)
		{
			symbolIndex = winbox.transform.parent.GetComponent<SlotSymbol>().symbolIndex;
			poolWinbox[symbolIndex].Add (winbox);
		}
		winbox.transform.parent = poolWinboxContainer.transform;
		winbox.transform.position = new Vector2(1000,1000);
		winbox.SetActive(false);
	}
	#endregion

	#region Debug

	public void log(string txt)
	{
		if (debugMode)
			Debug.Log(txt);
	}
	public void logError(string txt)
	{
		if (debugMode)
			Debug.LogError(txt);
	}
	public void logConfigError(string txt)
	{
		if (debugMode)
			Debug.LogError(txt);
	}

	#endregion
	#region State Function
	public void setState(SlotState newState)
	{
		if (OnSlotStateChangeFrom != null)
			OnSlotStateChangeFrom(state);
		state = newState;
		if (OnSlotStateChangeTo != null)
			OnSlotStateChangeTo(newState);
	}

	public int getSymbolCountCurrentlyTotal(int index)
	{
		int count = 0;
		
		for (int i = 0; i < reels.Count; i++)
		{
			foreach(GameObject symbol in reels[i].symbols)
			{
				if (symbol.GetComponent<SlotSymbol>().symbolIndex == index)
					count++;
			}
		}
		return count;
	}

	#endregion

	#region Slot Actions
	public void spinWithResult(int[,] result)
	{
		suppliedResult = result;
		useSuppliedResult = true;
		spin ();
	}
	public void spin() 
	{

		switch (state)
		{
		case SlotState.ready:
			if (GetComponent<SlotCredits>().placeBet())
			{
				if (OnSpinBegin != null)
					OnSpinBegin(GetComponent<SlotWins>().currentWin);

				//GetComponent<SlotWins>().reset();

				foreach(SetsWrapper set in symbolSets) set.scatterCount = 0;
				 
				for (int reelIndex = 0; reelIndex < reels.Count; reelIndex++) 
				{
					reels[reelIndex].GetComponent<SlotReel>().spinReel(spinTime + (spinTimeIncPerReel * reels[reelIndex].reelIndex - 1));
				}

				setState(SlotState.spinning);
			} else {
				if (OnSpinInsufficentCredits != null)
					OnSpinInsufficentCredits();
			}
		break;
		case SlotState.spinning:
			snap();
			break;
		case SlotState.snapping:
			break;
		case SlotState.playingwins:

			setState(SlotState.ready);
			spin ();
			break;
		}
	}

	void snap()
	{
		if (state != SlotState.spinning) return;

		if (OnSpinSnap != null)
			OnSpinSnap();

		setState(SlotState.snapping);

		for (int i = 0; i < reels.Count; i++)
		{
			reels[i].GetComponent<SlotReel>().snapReel();
		}
	}

	void calculateWins()
	{
		useSuppliedResult = false;

		int totalWon = GetComponent<SlotCompute>().calculateAllLinesWins();
		
		if (OnSpinDone != null)
			OnSpinDone(totalWon, GetComponent<SlotCredits>().totalBet());
		if (totalWon > 0)
		{
			setState(SlotState.playingwins);
			GetComponent<SlotCredits>().awardWin(totalWon);
		} else {
			setState(SlotState.ready);
			if (OnSpinDoneNoWins != null)
				OnSpinDoneNoWins();
		}

		if (OnAllWinsComputed != null)
			OnAllWinsComputed(totalWon, totalWon / refs.credits.totalBet());
	}

	#endregion

	#region Callback Passthrus

	public void computedWinLine(SlotWinData data)
	{
		if (OnLineWinComputed != null)
			OnLineWinComputed(data);
	}
	public void displayedWinLine(SlotWinData data, bool isFirstLoop)
	{
		if (OnLineWinDisplayed != null)
			OnLineWinDisplayed(data, isFirstLoop);
	}
	public void reelLanded(int reelIndex)
	{
		if (OnReelLand != null)
			OnReelLand(reelIndex);
	}
	public void scatterSymbolLanded(GameObject symbol, int scatterCount)
	{
		if (OnScatterSymbolLanded != null)
			OnScatterSymbolLanded(symbol, scatterCount);
	}
	public void scatterSymbolHit(SlotScatterHitData hit)
	{
		if (OnScatterSymbolHit != null)
			OnScatterSymbolHit(hit);
	}
	public void beginCreditWinCountOff(int totalWin)
	{
		if (OnBeginCreditWinCountOff != null)
			OnBeginCreditWinCountOff(totalWin);
	}
	public void beginCreditBonusCountOff(int totalWin)
	{
		if (OnBeginCreditBonusCountOff != null)
			OnBeginCreditBonusCountOff(totalWin);
	}
	public void completedBonusCreditCountOff()
	{
		if (OnCompletedBonusCreditCountOff != null)
			OnCompletedBonusCreditCountOff();
	}
	public void completedCreditCountOff(int totalWin)
	{
		if (OnCompletedCreditCountOff != null)
			OnCompletedCreditCountOff(totalWin);
	}
	public void beginDelayOnWin(SlotWinData data)
	{
		if (OnBeginDelayBetweenLineWinDisplayed != null)
			OnBeginDelayBetweenLineWinDisplayed(data);
	}
	public void anticipationScatterBegin(SlotScatterHitData hit)
	{
		if (OnAnticipationScatterBegin != null)
			OnAnticipationScatterBegin(hit);
	}
	public void completedWinDisplayCycle(int count)
	{
		if (OnWinDisplayedCycle != null)
			OnWinDisplayedCycle(count);
	}
	public void incrementedBet(int bet)
	{
		if (OnIncrementBetPerLine != null)
			OnIncrementBetPerLine(bet);

	}
	public void incrementedLinesPlayed(int linesPlayed)
	{
		if (OnIncrementLinesPlayed != null)
			OnIncrementLinesPlayed(linesPlayed);
		
	}
	#endregion

	#region Internal Callbacks
	void OnReelDoneSpinning(int reelIndex)
	{
		if (reelIndex == reels.Count)
		{
			calculateWins();
		}

	}
	
	#endregion

	void OnGUI()
	{
		if (OnSlotGUI != null)
			OnSlotGUI(state);
	}

}
