// Brad Lima - Bulkhead Studios 2014

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


enum PlayingWinsState {
	starting,
	playing,
	between,
	suspended
}
public class SlotWins : MonoBehaviour {

	public float showWinTime = 2.0f;
	public float delayBetweenShowingWins = 0.25f;
	public SlotWinData currentWin = null;

	private PlayingWinsState playingstate = PlayingWinsState.starting;
	private List<GameObject> winboxes = new List<GameObject>();
	private float winTimeout = 0;
	private int winLineOffset = 0;
	private int winDisplayCount = 0;
	private bool cycled = false;
	private Slot slot;

	private bool pause = false;
	#region Start
	void Start () {

		slot = GetComponent<Slot>();
	}
	#endregion

	#region Update
	void Update () {
		switch (GetComponent<Slot>().state)
		{
		case SlotState.playingwins:
			switch (playingstate)
			{
			case PlayingWinsState.starting:
				showWin();
				playingstate = PlayingWinsState.playing;
				break;
			case PlayingWinsState.playing:
				if (winTimeout > showWinTime)
				{
					//currentWin.readout = "";
					winTimeout = 0;
					releaseWinBoxes();
					playingstate = PlayingWinsState.between;
					slot.beginDelayOnWin(currentWin);
					return;
				}
				winTimeout += Time.deltaTime;
				break;
			case PlayingWinsState.between:
				if (pause)
				{
					playingstate = PlayingWinsState.suspended;
					return;
				}
				if (winTimeout > delayBetweenShowingWins)
				{
					playingstate = PlayingWinsState.playing;
					showWin();
					winTimeout = 0;
					return;
				}
				winTimeout += Time.deltaTime;
				break;
			case PlayingWinsState.suspended:
				if (!pause) playingstate = PlayingWinsState.between;
				break;
			}
			break;
		}
	}
	#endregion

	#region Misc
	public void reset()
	{
		playingstate = PlayingWinsState.starting;
		releaseWinBoxes();
		//CancelInvoke(resumeWinsInvoke);
		//currentWin.readout = "";
		winLineOffset = -1;
		winTimeout = 0;
		winDisplayCount = 0;

		cycled = false;
		currentWin = null;

		slot.refs.lines.hideLines ();

	}
	public void pauseWins()
	{
		pause = true;
	}
	public void resumeWins()
	{
		pause = false;
		//Invoke("resumeWinsInvoke", inTime);
	}
	void resumeWinsInvoke()
	{

	}
	public void releaseWinBoxes()
	{
		if (slot.usePool)
		{
			for(int i = 0; i < winboxes.Count; i++)
			{
				//GameObject wb = winboxes[i];
				slot.returnWinboxToPool(winboxes[i], currentWin.symbols[i].GetComponent<SlotSymbol>().symbolIndex);
			}
		} else {
			foreach(GameObject wb in winboxes) Destroy (wb);
		}
		winboxes.Clear();

	}
	#endregion

	#region Show Wins
	int findNextWin()
	{
		winLineOffset++;
		if (winLineOffset > slot.refs.compute.lineResultData.Count - 1) 
		{ 
			winLineOffset = 0;
			cycled = true;
			if (cycled) {
				winDisplayCount++;
				GetComponent<Slot>().completedWinDisplayCycle(winDisplayCount);
			}
		}

		return winLineOffset;
	}

	void showWin()
	{
		winLineOffset = findNextWin ();
		if (winLineOffset == -1) return;

		winTimeout = 0;

		currentWin = slot.refs.compute.lineResultData[winLineOffset];
		GetComponent<Slot>().displayedWinLine(currentWin, !cycled);

		for (int i = 0; i < currentWin.symbols.Count; i++)
		{
			if (slot.winboxPrefabs[currentWin.symbols[i].GetComponent<SlotSymbol>().symbolIndex] != null)
			{
				GameObject winbox;
				if (slot.usePool)
				{
					winbox = slot.getWinboxFromPool(currentWin.symbols[i].GetComponent<SlotSymbol>().symbolIndex);
				} else {
					winbox = (GameObject)Instantiate(slot.winboxPrefabs[currentWin.symbols[i].GetComponent<SlotSymbol>().symbolIndex]);
				}
				winbox.transform.localScale = Vector3.Scale(winbox.transform.localScale, transform.localScale);
				winbox.transform.parent = currentWin.symbols[i].transform;//.parent.transform;// GetComponent<Slot>().reels[i].symbols[pos[i]].transform;
				winbox.transform.position = currentWin.symbols[i].transform.position;
				winboxes.Add (winbox);
			}
		}
	}
	#endregion
}
