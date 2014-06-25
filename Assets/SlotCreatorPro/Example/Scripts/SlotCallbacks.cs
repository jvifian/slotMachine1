// Brad Lima - Bulkhead Studios 2014

using UnityEngine;
using System.Collections;

public class SlotCallbacks : MonoBehaviour {

	[HideInInspector]
	public Slot slot;

	#region Enable/Disable
	void OnEnable() {

		slot = GetComponent<Slot>();

		Slot.OnSlotUpdate += OnSlotUpdate;

		Slot.OnSlotStateChangeTo += OnSlotStateChangeTo;	
		Slot.OnSlotStateChangeFrom += OnSlotStateChangeFrom;	
		Slot.OnSlotGUI += OnSlotGUI;

		Slot.OnSpinBegin += OnSpinBegin;
		Slot.OnSpinInsufficentCredits += OnSpinInsufficentCredits;
		Slot.OnSpinSnap += OnSpinSnap;
		Slot.OnSpinDone += OnSpinDone;
		Slot.OnSpinDoneNoWins += OnSpinDoneNoWins;

		Slot.OnLineWinComputed += OnLineWinComputed;
		Slot.OnLineWinDisplayed += OnLineWinDisplayed;
		Slot.OnAllWinsComputed	+= OnAllWinsComputed;
		Slot.OnScatterSymbolHit += OnScatterSymbolHit;
		Slot.OnAnticipationScatterBegin += OnAnticipationScatterBegin;

		Slot.OnWinDisplayedCycle += OnWinDisplayedCycle;
	}

	#endregion

	void OnDisable() {

		Slot.OnSlotUpdate -= OnSlotUpdate;

		Slot.OnSlotStateChangeTo -= OnSlotStateChangeTo;	
		Slot.OnSlotStateChangeFrom -= OnSlotStateChangeFrom;
		Slot.OnSlotGUI -= OnSlotGUI;

		Slot.OnSpinBegin -= OnSpinBegin;
		Slot.OnSpinInsufficentCredits -= OnSpinInsufficentCredits;
		Slot.OnSpinSnap -= OnSpinSnap;
		Slot.OnSpinDone -= OnSpinDone;
		Slot.OnSpinDoneNoWins -= OnSpinDoneNoWins;

		Slot.OnLineWinDisplayed -= OnLineWinDisplayed;
		Slot.OnLineWinDisplayed -= OnLineWinDisplayed;
		Slot.OnAllWinsComputed -= OnAllWinsComputed;
		Slot.OnScatterSymbolHit -= OnScatterSymbolHit;
		Slot.OnAnticipationScatterBegin -= OnAnticipationScatterBegin;

		Slot.OnWinDisplayedCycle -= OnWinDisplayedCycle;
	}

	#region Update Callback

	private void OnSlotUpdate()
	{
	}
	#endregion


	#region State Callbacks 

	private void OnSlotStateChangeFrom(SlotState state)
	{
		slot.log ("onSlotStateChangeFrom " + state);
		switch (state)
		{
		case SlotState.playingwins:
			break;
		case SlotState.ready:
			break;
		case SlotState.snapping:
			break;
		case SlotState.spinning:
			break;
		}
	}
	private void OnSlotStateChangeTo(SlotState state)
	{
		slot.log ("OnSlotStateChangeTo " + state);
		switch (state)
		{
		case SlotState.playingwins:
			break;
		case SlotState.ready:
			break;
		case SlotState.snapping:
			break;
		case SlotState.spinning:
			break;
		}
	}
	#endregion

	#region Spin Callbacks
	private void OnSpinBegin(SlotWinData data)
	{
		slot.refs.lines.hideLines ();
		slot.log ("OnSpinBegin Callback");
	}

	private void OnSpinInsufficentCredits()
	{
		slot.log ("OnSpinInsufficentCredits Callback");
	}

	private void OnSpinSnap()
	{
		slot.log ("OnSpinSnap Callback");
	}

	private void OnSpinDone(int totalWon, int timesWin)
	{
		slot.log ("OnSpinDone Callback");
	}

	private void OnSpinDoneNoWins()
	{
		slot.log ("OnSpinDoneNoWins Callback");
	}
	#endregion

	#region Win Callbacks
	private void OnLineWinComputed(SlotWinData win)
	{
		slot.log ("OnLineWinComputed Callback");
		slot.log ("win line " + win.lineNumber + " :: set: " + win.setName + " (" + win.setIndex + ") paid: " + win.paid + " matches: " + win.matches);
	}

	private void OnLineWinDisplayed(SlotWinData win, bool isFirstLoop)
	{
		slot.log ("OnLineWinDisplayed Callback");
		slot.log ("win line " + win.lineNumber + " :: set: " + win.setName + " (" + win.setIndex + ") paid: " + win.paid + " matches: " + win.matches);
	}

	private void OnAllWinsComputed(int totalWon, int timesBet)
	{
		slot.log ("OnAllWinsComputed Callback");
	}

	private void OnScatterSymbolHit(SlotScatterHitData hit)
	{
		slot.log ("OnScatterSymbolHit Callback");
		hit.symbol.transform.eulerAngles = new Vector2(0,0);
	}

	private void OnAnticipationScatterBegin(SlotScatterHitData hit)
	{
		slot.log ("OnAnticipationScatterBegin Callback");
	}

	void OnWinDisplayedCycle (int count)
	{
		slot.log ("OnWinDisplayedCycle Callback");
	}

	#endregion

	#region 

	void OnSlotGUI(SlotState state)
	{

	}
	#endregion
}
