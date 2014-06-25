using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SlotCompute : MonoBehaviour {

	public List<SlotWinData> lineResultData = new List<SlotWinData>();

	private Slot slot;


	#region Start
	void Start () {
		slot = GetComponent<Slot>();
	}
	#endregion

	#region Calc Eval

	public int calculateAllLinesWins()
	{
		lineResultData.Clear ();

		int totalWin = 0;
		if ((slot.lines.Count == 0) || (slot.refs.credits.linesPlayed > slot.lines.Count))
		{
			slot.logConfigError(SlotErrors.NO_LINES);
			return 0;
		}
		for (int lineToCalculate = 0; lineToCalculate < slot.GetComponent<SlotCredits>().linesPlayed; lineToCalculate++)
		{
			totalWin += calculatePayForLine(lineToCalculate);
		}

		totalWin += calculateScatterPays();

		return totalWin;
	}

	int calculateScatterPays()
	{
		if (slot.symbolSets.Count == 0)
		{
			slot.logConfigError(SlotErrors.NO_SYMBOLSETS);
			return 0;
		}
		int totalWon = 0;
		for(int currentSymbolSetIndex = 0; currentSymbolSetIndex < slot.symbolSets.Count; currentSymbolSetIndex++)
		{
			SetsWrapper currentSet = slot.symbolSets[currentSymbolSetIndex];
			if (currentSet.typeofSet != SetsType.scatter) continue;

			int matches = 0;
			SlotWinData winData = new SlotWinData(-1);

			for (int reel = 0; reel < slot.numberOfReels; reel++)
			{
				for (int range = slot.reelIndent; range < (slot.reelHeight - slot.reelIndent); range++)
				{
					int symbolIndexToCompare = slot.reels[reel].symbols[range].GetComponent<SlotSymbol>().symbolIndex;
					foreach(int symbolInSet in currentSet.symbols)
					{
						if (symbolInSet == symbolIndexToCompare) {
							matches++;
							winData.symbols.Add(slot.reels[reel].symbols[range]);
							break;
						}
					}

				}

			}

			if (matches > 0)
			{
				if (slot.setPays[currentSymbolSetIndex].pays.Count < matches)
				{
					slot.logConfigError(SlotErrors.CLAMP_SCATTER);
					return 0;
				}
				int pay = slot.setPays[currentSymbolSetIndex].pays[matches-1] * slot.GetComponent<SlotCredits>().betPerLine;
				if (pay > 0)
				{
					winData.lineNumber = -1;
					winData.matches = matches;
					winData.paid = pay;
					winData.setType = currentSet.typeofSet;
					winData.setIndex = currentSymbolSetIndex;
					winData.setName = slot.symbolSetNames[winData.setIndex];
					winData.readout = winData.matches + " " + winData.setName + " Scatter pays " + winData.paid;
					lineResultData.Add (winData);
					
					slot.computedWinLine(winData);
					totalWon += lineResultData[lineResultData.Count-1].paid;
				}

			}
		}
		return totalWon;
	}

	int calculatePayForLine(int lineNumber) {

		int highMatches = 0;
		int highPaid = 0;
		int highSet = 0;

		if (slot.symbolSets.Count == 0)
		{
			slot.logConfigError(SlotErrors.NO_SYMBOLSETS);
			return 0;
		}

		SlotWinData winData = new SlotWinData(lineNumber);

		List<int> linePositions = slot.lines[lineNumber].positions;

		for(int currentSymbolSetIndex = 0; currentSymbolSetIndex < slot.symbolSets.Count; currentSymbolSetIndex++)
		{
			SetsWrapper currentSet = slot.symbolSets[currentSymbolSetIndex];
			if (currentSet.typeofSet != SetsType.normal) continue;

			int numberOfMatchingSymbols = 0;
			List<GameObject> winningSymbols = new List<GameObject>();

			for (int currentLinePosition = 0; currentLinePosition < linePositions.Count; currentLinePosition++)
			{
				int symbolIndexToCompare = slot.reels[currentLinePosition].symbols[linePositions[currentLinePosition]].GetComponent<SlotSymbol>().symbolIndex;

				bool foundMatchingSymbolInSet = false;
				foreach(int symbolInSet in currentSet.symbols)
				{
					if ((symbolInSet == symbolIndexToCompare) || (slot.symbolPrefabs[symbolIndexToCompare].GetComponent<SlotSymbol>().isWild && currentSet.allowWilds))
					{ 
						foundMatchingSymbolInSet = true; 
						numberOfMatchingSymbols++; 
						winningSymbols.Add(slot.reels[currentLinePosition].symbols[linePositions[currentLinePosition]]);
						break;
					}
				}
				// if no match is found, abort the search, since the set is no longer consecutive
				if (!foundMatchingSymbolInSet)
					break;
			}
			if ((numberOfMatchingSymbols >= highMatches) && numberOfMatchingSymbols > 0)
			{
				int pay = slot.setPays[currentSymbolSetIndex].pays[numberOfMatchingSymbols-1] * slot.refs.credits.betPerLine;

				if (pay > highPaid)
				{
					highMatches = numberOfMatchingSymbols;
					highPaid = pay;
					highSet = currentSymbolSetIndex;
					winData.symbols = winningSymbols;
				}
			}
		}

		if (highPaid > 0) {
			winData.lineNumber = lineNumber;
			winData.matches = highMatches;
			winData.paid = highPaid;
			winData.setIndex = highSet;
			winData.setType = slot.symbolSets[winData.setIndex].typeofSet;
			winData.setName = slot.symbolSetNames[winData.setIndex];
			winData.readout = winData.matches + " " + winData.setName + " on line " + (winData.lineNumber + 1) + " pays " + winData.paid;
			lineResultData.Add (winData);

			slot.computedWinLine(winData);
		}
		return highPaid;
	}
	#endregion

}
