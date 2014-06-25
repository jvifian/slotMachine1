using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using System.Collections.Generic;

public class SlotCredits : MonoBehaviour {

	[HideInInspector]
	public bool persistant = true;
	[HideInInspector]
	public int betPerLineDefaultIndex;
	//public int maxBetPerLine = 5;
	[HideInInspector]
	public int betPerLineIndex = 0;
	[HideInInspector]
	public int credits;
	[HideInInspector]
	public int queue;
	[HideInInspector]
	public int lastWin;
	[HideInInspector]
	public int betPerLine = 1;
	[HideInInspector]
	public int linesPlayed = 1;

	public int totalIn;
	public int totalOut;

	private Tweener creditsTween;

	private Slot slot;


	void Awake () {
		restore ();
	}
	void Start () {
	}

	// Update is called once per frame
	void Update () {
	
	}

	#region Save/Restore machine
	public void restore() 
	{
		slot = GetComponent<Slot>();
		if (persistant)
		{
			slot.log ("restoring slot settings");
			credits = PlayerPrefs.GetInt(slot.name + "_credits", credits);
			betPerLineIndex = PlayerPrefs.GetInt(slot.name + "_betPerLineIndex", betPerLineIndex);
			betPerLine = slot.betsPerLine[betPerLineIndex].value; //PlayerPrefs.GetInt(slot.name + "_betPerLine", betPerLine);
			linesPlayed = PlayerPrefs.GetInt(slot.name + "_linesPlayed", linesPlayed);

			for (int index = 0; index < slot.betsPerLine.Count; index++)
			{
				slot.betsPerLine[index].canBet = PlayerPrefsX.GetBool(slot.name + "_betsPerLine" + index, slot.betsPerLine[index].canBet);
			}
			if (linesPlayed > slot.lines.Count) linesPlayed = slot.lines.Count;
			//if (betPerLine > maxBetPerLine) betPerLine = maxBetPerLine;
		} else {
			if (betPerLineDefaultIndex > slot.betsPerLine.Count) { slot.logConfigError("Your machine default bet per line index is greater than the actual bets per line"); return; }
			betPerLine = slot.betsPerLine[betPerLineDefaultIndex].value;
			betPerLineIndex = betPerLineDefaultIndex;
		}
	}

	void save()
	{
		PlayerPrefs.SetInt(slot.name + "_credits", credits);
		PlayerPrefs.SetInt(slot.name + "_betPerLineIndex", betPerLineIndex);
		PlayerPrefs.SetInt(slot.name + "_betPerLine", betPerLine);
		PlayerPrefs.SetInt(slot.name + "_linesPlayed", linesPlayed);
		for (int index = 0; index < slot.betsPerLine.Count; index++)
		{
			PlayerPrefsX.SetBool(slot.name + "_betsPerLine" + index, slot.betsPerLine[index].canBet);
		}
	}
	#endregion

	#region Deposit and Withdraw Credits
	public void setCredits(int creds)
	{ 
		slot.log ("setting credits = " + creds);
		credits = creds;
	}
	public void depositCredits(int deposit)
	{
		slot.log ("setting slot credits = " + deposit);
		credits += deposit;
		//HOTween.To (this, 2.0f, new TweenParms().Prop ("credits", credits + deposit).Ease(EaseType.EaseOutBack));
	}
	
	public int withdrawCredits()
	{
		return credits;
	}
	#endregion

	#region Betting functions
	public int totalCredits()
	{
		return credits + queue;
	}
	public int totalBet()
	{
		return betPerLine * linesPlayed;
	}

	public void betMaxPerLine()
	{
		int max = -1;
		for (int index = 0; index < slot.betsPerLine.Count; index++)
		{
			BetsWrapper bet = slot.betsPerLine[index];
			if ((bet.canBet) && (bet.value > max))
			{
				max = index;
			}
		}
		betPerLineIndex = max;
		betPerLine = slot.betsPerLine[max].value;
	}
	public void incBetPerLine()
	{
		slot = GetComponent<Slot>();
		switch (slot.state) 
		{
		case SlotState.ready:

			betPerLineIndex++;
			if (betPerLineIndex > slot.betsPerLine.Count-1) betPerLineIndex = 0;
			while (!slot.betsPerLine[betPerLineIndex].canBet)
			{
				betPerLineIndex++;
				if (betPerLineIndex > slot.betsPerLine.Count-1) betPerLineIndex = 0;
			}
			betPerLine = slot.betsPerLine[betPerLineIndex].value;
			slot.incrementedBet(betPerLine);

			break;
		case SlotState.playingwins:
			slot.setState(SlotState.ready);
			incBetPerLine();
			break;
		}

		save();
	}
	
	public void incLinesPlayed()
	{
		switch (slot.state) 
		{
		case SlotState.ready:
			linesPlayed++;
			if (linesPlayed > slot.lines.Count) { linesPlayed = 1; }
			slot.incrementedLinesPlayed(linesPlayed);
			slot.refs.lines.displayLines(linesPlayed);
			break;
		case SlotState.playingwins:
			slot.setState(SlotState.ready);
			slot.refs.wins.reset();
			incLinesPlayed();
			break;
		}

		save();
}

	public bool placeBet()
	{
		if (totalCredits() < totalBet()) 
		{
			return false;
		}
		credits -= totalBet ();
		totalIn += totalBet ();
		save();
		return true;
	}

	public void awardWin(int amount)
	{
		if (creditsTween != null)
			creditsTween.Complete();

		lastWin = amount;
		//DebugX.log (DebugMessageType.Critical, "Awarding credits:" + lastWin);
		//credits += lastWin;
		float countOffTime = Mathf.Clamp(amount * 0.05f, 2.0f,3.0f);
		creditsTween = HOTween.To (this,countOffTime,new TweenParms().Prop ("queue", amount).OnComplete(completedCreditTween));
		slot.beginCreditWinCountOff(lastWin);
		//queue += amount;
		//credits += queue;
		//queue = 0;
		totalOut += amount;
		save();
	}
	public void awardBonus(int amount, bool addToTotalIn=true)
	{
		if (creditsTween != null)
			creditsTween.Complete();

		float countOffTime = Mathf.Clamp(amount * 0.0005f, 2.0f,3.0f);
		creditsTween = HOTween.To (this,countOffTime,new TweenParms().Prop ("queue", amount).OnComplete(completedBonusCreditTween));
		slot.beginCreditBonusCountOff(lastWin);

		if (addToTotalIn)
			totalOut += amount;
		save();
	}

	public void finishCreditCount()
	{
		if (creditsTween == null) return;
		if (!creditsTween.isComplete) creditsTween.Complete();
	}
	void completedBonusCreditTween()
	{
		credits += queue;
		queue = 0;
		slot.completedBonusCreditCountOff();
	}
	void completedCreditTween()
	{
		credits += queue;
		queue = 0;
		slot.completedCreditCountOff(lastWin);
	}
	public void enableBet(int index)
	{
		slot.betsPerLine[index].canBet = false;
		save();
	}
	public void disableBet(int index)
	{
		slot.betsPerLine[index].canBet = true;
		save();
	}

	#endregion
}
