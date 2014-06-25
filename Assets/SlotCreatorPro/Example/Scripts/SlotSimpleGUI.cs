// Brad Lima - Bulkhead Studios 2014
//
// simple gui class for use with the slot machine examples
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotSimpleGUI : MonoBehaviour {

	public GUISkin skin;

	[HideInInspector]
	Slot slot;
	bool togglePays;


	void Awake() {
		slot = GetComponent<Slot>();
	}

	void showBetAndLinesButtons()
	{
		if (GUI.Button(new Rect(Screen.width-250, Screen.height-50, 100, 50), "Lines Played: " +  slot.refs.credits.linesPlayed.ToString ()))
		{
			slot.refs.credits.incLinesPlayed();
		}
		if (GUI.Button(new Rect(Screen.width-150, Screen.height-50, 100, 50), "Bet Per Line: " +  slot.refs.credits.betPerLine.ToString ()))
		{
			slot.refs.credits.incBetPerLine();
		}

		if (GUI.Button(new Rect(Screen.width-585, Screen.height-160, 175, 100), "Get Credits"))
		{
			slot.refs.credits.depositCredits(100);
		}
		string payslabel = (togglePays)?"Hide Pays":"Show Pays";
		if (GUI.Button(new Rect(Screen.width-250, 25, 100, 50), payslabel))
		{
			togglePays = !togglePays;
		}

	}

	void showPays(int windowID)
	{
		GUI.skin.label.alignment = TextAnchor.MiddleLeft;
		GUI.contentColor = Color.black;
		for (int setIndex = 0; setIndex < slot.symbolSets.Count; setIndex++)
		{
			GUI.Label(new Rect(25,60+(30*setIndex),200,50), slot.symbolSetNames[setIndex]);
			for (int payIndex = 0; payIndex < slot.setPays[setIndex].pays.Count; payIndex++)
			{
				if (setIndex == 0)
				{
					GUI.Label(new Rect(250+(50 * payIndex),30,50,50), (payIndex+1).ToString());
				}
				string pay = slot.setPays[setIndex].pays[payIndex].ToString();
				if (pay == "0") pay = "-";
				GUI.Label(new Rect(250+(50 * payIndex),60+(30*setIndex),50,50), pay);
			}
		}
	}
	void OnGUI () {

		GUI.skin = skin;

		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.skin.label.fontSize = 18;

		if (togglePays)
		{
			Rect windowRect = new Rect(Screen.width*0.1f, Screen.height*0.1f, Screen.width*0.8f, Screen.height*0.8f);
			windowRect = GUI.Window(0, windowRect, showPays, "Pays");
		}
//spin
		string spinText = "";
		switch (slot.state)
		{
		case SlotState.playingwins:
			if (slot.refs.wins.currentWin == null) return;
			GUI.Label(new Rect(0,Screen.height-125,Screen.width,50), slot.refs.wins.currentWin.readout.ToString ());
			GUI.Label(new Rect(0,Screen.height-75,Screen.width,50), "Total Won: " +  slot.refs.credits.lastWin.ToString ());
			showBetAndLinesButtons();
			break;
		case SlotState.ready:
			showBetAndLinesButtons();
			break;
		case SlotState.snapping:
			break;
		case SlotState.spinning:
//stop
			spinText = "";
			break;

		}
		if (GUI.Button(new Rect(Screen.width-865, Screen.height-160, 175, 100), spinText))
		{
//play audio
			audio.Play();
			slot.spin();
		}
		
		GUI.skin.label.fontSize = 22;
		GUI.Label(new Rect(0,120,Screen.width,50), "Credits: " +  slot.refs.credits.credits.ToString (), GUI.skin.label);
		GUI.Label(new Rect(0,Screen.height-50,Screen.width,50), "Total Bet: " +  slot.refs.credits.totalBet ().ToString ());
	}
}
