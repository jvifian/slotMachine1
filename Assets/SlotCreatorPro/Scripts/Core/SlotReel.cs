// Brad Lima - Bulkhead Studios 2014

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using System;
using System.Linq;

public class SlotReel : MonoBehaviour {

	public static event Action<int> OnReelDoneSpinning;

	public int reelIndex;
	public float speed;

	[HideInInspector]
	public List<GameObject> symbols = new List<GameObject>();
	List<Transform> symbolsParents = new List<Transform>();
	List<Transform> symbolsChildren = new List<Transform>();

	private bool snapped = false;
	private bool stopped = false;

	//private List<List<int>> cumulativeFrequencyLists = new List<List<int>>();

	private List<int> cumulativeFrequencyList = new List<int>();
	private int totalFrequency;

	public float symbolHeight;
	public float symbolWidth;
	private float heightOffset;
	private float widthOffset;
	Slot slot;

	private Tweener spinTween;
	private bool anticipation;

	private int symbolsSpinRemaining;
	#region Config
	void Awake ()
	{
	}
	void Start () 
	{

	}

	void OnEnable()
	{
	
		slot = transform.parent.gameObject.GetComponent<Slot>();

		if (slot.symbolPrefabs.Count == 0)
		{
			slot.logConfigError(SlotErrors.NO_SYMBOLS);
			return;
		}

		cacheSymbolFrequency();

		if (slot.symbolPrefabs[0] == null) return;
		GameObject symb = (GameObject)Instantiate(slot.symbolPrefabs[0]);
		symb.transform.localScale = Vector3.Scale(symb.transform.localScale, transform.parent.transform.localScale);
		//symb.transform.localRotation = transform.parent.transform.rotation;
		if (symb.GetComponent<SpriteRenderer>())
		{
			Vector3 size = symb.GetComponent<SpriteRenderer>().sprite.bounds.size;
			symbolHeight = size.y * symb.transform.localScale.y;
			symbolWidth = size.x * symb.transform.localScale.x;
		} else
		if (symb.GetComponent<MeshFilter>())
		{
			symbolHeight = symb.GetComponent<MeshFilter>().mesh.bounds.size.y * slot.transform.localScale.y;
			symbolWidth = symb.GetComponent<MeshFilter>().mesh.bounds.size.x * slot.transform.localScale.y;
		} else {
			slot.logConfigError(SlotErrors.MISSING_RENDERER);
			return;
		}
		heightOffset = -transform.parent.transform.position.y + (symbolHeight * (slot.reelHeight / 2));
		widthOffset = -transform.parent.transform.position.x + (symbolWidth * (slot.numberOfReels / 2));
		Destroy (symb);
		
		createReelSymbols();
	}
	#endregion

	#region Create Reel
	public void createReelSymbols()
	{
		transform.position = new Vector3(-widthOffset + (symbolWidth * (reelIndex - 1)),transform.position.y, transform.position.z);
		for (int i = 0; i < slot.reelHeight; i++)
		{
			createSymbol(i);
		}	
	}

	void cacheSymbolFrequency()
	{
		cumulativeFrequencyList.Clear();
		totalFrequency = 0;
		for (int index = 0; index < slot.symbolFrequencies.Count; index++)
		{
			if (slot.symbolPrefabs[index] == null) continue;
			SlotSymbol symbol = slot.symbolPrefabs[index].GetComponent<SlotSymbol>();
			if (symbol.perReelFrequency)
			{
				totalFrequency += slot.reelFrequencies[index].freq[reelIndex-1];
			} else {
				totalFrequency += slot.symbolFrequencies[index];
			}
			cumulativeFrequencyList.Add ( totalFrequency ); 
		}

	}
	int getSymbolCountCurrentlyOnReel(int index)
	{
		int count = 0;
		foreach(GameObject symbol in symbols)
		{
			if (symbol.GetComponent<SlotSymbol>().symbolIndex == index)
				count++;
		}
		return count;
	}

	public int getSymbol()
	{
		int chosen = -1;
		while (chosen == -1)
		{
			//int selectedFrequency = UnityEngine.Random.Range(1,totalFrequency+1);
			uint selectedFrequency = RNGManager.getRandomRange(slot.activeRNG, 1, totalFrequency+1);
			for (int index = 0; index < cumulativeFrequencyList.Count; index++)
			{
				if (selectedFrequency <= cumulativeFrequencyList[index]) { chosen = index; break; }
			}
			int maxPerReel = slot.symbolPrefabs[chosen].GetComponent<SlotSymbol>().clampPerReel;
			if (maxPerReel > 0)
			{
				if (getSymbolCountCurrentlyOnReel(chosen) >= maxPerReel) { chosen = -1; continue; }
				int maxTotal = slot.symbolPrefabs[chosen].GetComponent<SlotSymbol>().clampTotal;
				if (maxTotal > 0)
					if (slot.getSymbolCountCurrentlyTotal(chosen) >= maxTotal) chosen = -1;
			}
		}

		return chosen;
	}

	void createSymbol(int slotIndex)
	{
		int symbolIndex;
		if (slot.useSuppliedResult)
		{
			if ((symbolsSpinRemaining >= slot.reelIndent) && (symbolsSpinRemaining <= (slot.reelHeight - (slot.reelIndent * 2))))
			{
				symbolIndex = slot.suppliedResult[reelIndex-1,symbolsSpinRemaining-1];
			} else {
				symbolIndex = getSymbol();
			}
		} else {
			symbolIndex = getSymbol();
		}


		GameObject symb;
		if (slot.usePool) 
		{
			symb = getFromPool(symbolIndex);
		} else {
			symb = (GameObject)Instantiate(slot.symbolPrefabs[symbolIndex]);
			symb.GetComponent<SlotSymbol>().symbolIndex = symbolIndex;
			symb.transform.localScale = Vector3.Scale(symb.transform.localScale, transform.parent.transform.localScale);
			//symb.transform.localRotation = transform.parent.transform.rotation;
		}

		symb.transform.parent = transform;
		if (symb.GetComponent<SpriteRenderer>())
			symb.transform.localPosition = new Vector3(0,-heightOffset + (slotIndex * symbolHeight - transform.position.y), 0);
		if (symb.GetComponent<MeshFilter>())
			symb.transform.localPosition = new Vector3(0,-heightOffset + (slotIndex * symbolHeight - transform.position.y), 0);

		symbols.Insert(0,symb);
	}
	#endregion

	#region Actions
	public void spinReel(float spinTime)
	{
		retatchBackgrounds();
		cacheSymbolFrequency();

		snapped = false;
		stopped = false;
		speed = slot.spinningSpeed;

		symbolsSpinRemaining = (int)(spinTime / speed);
		//spinTween = HOTween.To (this,spinTime,new TweenParms().Prop("speed", speed).OnComplete(finishSpinning));
		HOTween.To (transform,speed,new TweenParms().Prop ("position", new Vector3(0,-symbolHeight,0), true).OnComplete(OnNextSymbol).Ease (EaseType.Linear));
	}

	public void snapReel()
	{
		if (snapped) return;
		snapped = true;
		if (slot.useSuppliedResult)
		{
			symbolsSpinRemaining = (int)(slot.reelHeight - slot.reelIndent);
		} else {
			//HOTween.Kill (spinTween);
			symbolsSpinRemaining = 1;
			//HOTween.To (this,slot.easeOutTime,new TweenParms().Prop("speed", slot.spinningSpeed).OnComplete(finishSpinning));
		}
	}
	#endregion

	#region Symbol Misc
	GameObject getFromPool(int symbolIndex)
	{
		GameObject symbol = slot.getSymbolFromPool(symbolIndex);
		return symbol;
	}
	void returnToPool()
	{
		if (symbols.Count == 0)
		{
			slot.logConfigError(SlotErrors.NO_SYMBOLS);
			return;
		}
		GameObject symb = symbols[symbols.Count-1];

		slot.returnSymbolToPool(symb);
	}

	void detatchBackgrounds()
	{
		foreach(GameObject symbol in symbols)
		{
			
			foreach(Transform child in symbol.transform) {
				symbolsParents.Add (child.parent);
				child.parent = child.parent.transform.parent;
				symbolsChildren.Add(child);
			}
		}
	}

	void retatchBackgrounds()
	{
		for(int i = 0; i < symbolsChildren.Count; i++) symbolsChildren[i].parent = symbolsParents[i];
		symbolsChildren.Clear();
		symbolsParents.Clear() ;
	}

	#endregion

	#region HOTween Callbacks
	void OnNextSymbol()
	{
		if (symbols.Count == 0)
		{
			slot.logConfigError(SlotErrors.NO_SYMBOLS);
			return;
		}

		if (slot.usePool) returnToPool();
		else 
			Destroy (symbols[symbols.Count-1]);
		symbols.RemoveAt(symbols.Count-1);

		createSymbol(slot.reelHeight-1);

		symbolsSpinRemaining--;
		if (symbolsSpinRemaining == 0) {
			stopped = true;
		}

		//if (reelIndex == 3)
		//	Debug.Log("remaining=" + symbolsSpinRemaining);

		if (stopped)
		{
			HOTween.To (transform,slot.easeOutTime,new TweenParms().Prop ("position", new Vector3(0,-symbolHeight,0), true).OnComplete(OnReelStopped).Ease (slot.reelEase));
			Invoke("checkScatterLanded", slot.easeOutTime / 2.0f);
			slot.reelLanded(reelIndex);

		} else {
			HOTween.To (transform,speed,new TweenParms().Prop ("position", new Vector3(0,-symbolHeight,0), true).OnComplete(OnNextSymbol).Ease (EaseType.Linear));
		}
	}

	void finishSpinning()
	{
		stopped = true;
	}

	void OnReelStopped()
	{
		if (slot.usePool) returnToPool();
		else 
			Destroy (symbols[symbols.Count-1]);
		symbols.RemoveAt(symbols.Count-1);

		createSymbol(slot.reelHeight-1);

		if (OnReelDoneSpinning != null)
			OnReelDoneSpinning(reelIndex);

		detatchBackgrounds();

		inlineScatterCalc();
		
	}
	
	void checkScatterLanded()
	{

		for(int currentSymbolSetIndex = 0; currentSymbolSetIndex < slot.symbolSets.Count; currentSymbolSetIndex++)
		{
			
			SetsWrapper currentSet = slot.symbolSets[currentSymbolSetIndex];
			if (currentSet.typeofSet != SetsType.scatter) continue;
			foreach(int symbol in currentSet.symbols)
			{
				for (int i = slot.reelIndent - 1; i < (slot.reelHeight - slot.reelIndent) - 1; i++)
				{
					int reelSymbolIndex = symbols[i].GetComponent<SlotSymbol>().symbolIndex;
					if (reelSymbolIndex == symbol)
					{
						slot.scatterSymbolLanded(symbols[i], currentSet.scatterCount + 1);
					}
				}
			}
		}
	}

	void inlineScatterCalc()
	{
		for(int currentSymbolSetIndex = 0; currentSymbolSetIndex < slot.symbolSets.Count; currentSymbolSetIndex++)
		{
			
			SetsWrapper currentSet = slot.symbolSets[currentSymbolSetIndex];
			if (currentSet.typeofSet != SetsType.scatter) continue;
			int matches = 0;
			SlotScatterHitData hit = new SlotScatterHitData(reelIndex);

			foreach(int symbol in currentSet.symbols)
			{
				for (int i = slot.reelIndent; i < (slot.reelHeight - slot.reelIndent); i++)
				{
					int reelSymbolIndex = symbols[i].GetComponent<SlotSymbol>().symbolIndex;
					if (reelSymbolIndex == symbol)
					{
						currentSet.scatterCount++;
						matches++;
						hit.hits = currentSet.scatterCount;
						hit.setIndex = currentSymbolSetIndex;
						hit.setType = SetsType.scatter;
						hit.setName = slot.symbolSetNames[hit.setIndex];
						hit.symbol = symbols[i];
						slot.scatterSymbolHit(hit);
					}
				}
			}
			// anticipation
			if (currentSet.scatterCount > 0)
			{
				if ((currentSet.scatterCount < slot.numberOfReels) && (reelIndex < slot.numberOfReels))
				{
					if ((slot.setPays[currentSymbolSetIndex].anticipate[currentSet.scatterCount-1]) == true)
					{
						for (int i = reelIndex; i < slot.numberOfReels; i++)
						{
							slot.reels[i].symbolsSpinRemaining += 10;
							slot.anticipationScatterBegin(hit);
						}
					}
				}
			}
		}
	}
	#endregion

}
