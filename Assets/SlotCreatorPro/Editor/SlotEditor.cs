using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using Holoville.HOTween;

[CustomEditor ( typeof(Slot))]
[Serializable]
public class SlotEditor : Editor 
{

	List<bool> setFoldouts = new List<bool>();
	List<string>configIssues = new List<string>();

	private static MonoScript callbackScript = null;

	Slot slot;

	void OnEnable()
	{
		slot = (Slot)target;
		slot.GetComponent<SlotWins>().hideFlags = HideFlags.HideInInspector;
		slot.GetComponent<SlotCredits>().hideFlags = HideFlags.HideInInspector;
		slot.GetComponent<SlotCompute>().hideFlags = HideFlags.HideInInspector;
		slot.GetComponent<SlotComputeEditor>().hideFlags = HideFlags.HideInInspector;
		slot.GetComponent<SlotLines>().hideFlags = HideFlags.HideInInspector;
	}

	#region Main GUI
	public override void OnInspectorGUI() { 

		configIssues.Clear();

		Undo.RecordObject(slot, "Slot Creator Change");
		Undo.RecordObject(slot.GetComponent<SlotWins>(), "Slot Creator Change");
		Undo.RecordObject(slot.GetComponent<SlotCredits>(), "Slot Creator Change");

		for (int i=0; i < slot.symbolSetNames.Count; i++)
		{
			setFoldouts.Add (false);
		}

		EditorGUILayout.BeginVertical("Box");
		//GUI.color = Color.blue;
		EditorGUILayout.LabelField("Reels:" + (slot.reelHeight - (slot.reelIndent * 2)).ToString() + "x" + slot.numberOfReels + " " +
		                           "Symbols:" + slot.symbolPrefabs.Count + " " +
		                           "Pays:" + slot.symbolSets.Count + " " +
		                           "Lines:" + slot.lines.Count + " " +
		                           "(" + (slot.edsave.returnPercent * 100) + "%)"
		                           , EditorStyles.miniLabel);
		//GUI.color = Color.white;
		EditorGUILayout.EndVertical();
		slot.edsave.showBasicSettingsPanel = EditorGUILayout.Foldout(slot.edsave.showBasicSettingsPanel, new GUIContent("Basic Settings", "The core settings for the slot."), EditorStyles.foldout);
		if (slot.edsave.showBasicSettingsPanel)
		{
			foldoutBasicSettings();
		}


		slot.edsave.showSymbolSetupPanel = EditorGUILayout.Foldout(slot.edsave.showSymbolSetupPanel, new GUIContent("Symbol Setup", "Here you will define your symbols and how often they occur."), EditorStyles.foldout);
		if (slot.edsave.showSymbolSetupPanel)
		{
			foldoutSymbolSetup();
		}

		slot.edsave.showSymbolSetsPanel = EditorGUILayout.Foldout(slot.edsave.showSymbolSetsPanel, new GUIContent("Symbol Sets Setup", "This is where you will define all matching symbols."), EditorStyles.foldout);
		if (slot.edsave.showSymbolSetsPanel)
		{
			foldoutSymbolSetsSetup();
		}

		slot.edsave.showLinesSetupPanel = EditorGUILayout.Foldout(slot.edsave.showLinesSetupPanel, new GUIContent("Payline Setup", "This is where you will define your paylines."), EditorStyles.foldout);
		if (slot.edsave.showLinesSetupPanel)
		{
			foldoutLinesSetup();
		}

		// basic settings
		slot.edsave.showReturnPanel = EditorGUILayout.Foldout(slot.edsave.showReturnPanel, new GUIContent("Math (RP: " + (slot.edsave.returnPercent * 100) + "%)", "You can view the slot machines return percentage here."), EditorStyles.foldout);
		if (slot.edsave.showReturnPanel)
		{
			foldoutReturn();
		}
		if (GUI.changed)
		{
			EditorUtility.SetDirty(slot);
			if (slot.edsave.autoCompute)
			{
				calcReturn();
			}

		}

		checkErrors();
		slot.edsave.showNotificationsPanel = EditorGUILayout.Foldout(slot.edsave.showNotificationsPanel, new GUIContent("Notifications (" + configIssues.Count + ")", "Any issues with your slot's setup will be shown here."), EditorStyles.foldout);

		if (slot.edsave.showNotificationsPanel)
		{
			EditorGUI.indentLevel++;
			drawNotifications();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.HelpBox(SlotGlobals.PRODUCT_NAME + " - v" + SlotGlobals.PRODUCT_VERSION + " - " + SlotGlobals.COMPANY, MessageType.None);
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel--;
		}
		if (slot.transform.rotation != Quaternion.identity)
			slot.transform.rotation = Quaternion.identity;

	}
	#endregion

	#region Math Section
	void calcReturn()
	{
		slot.edsave.returnPercent = 0;
		slot.edsave.returnTotalWon = slot.GetComponent<SlotComputeEditor>().calculateReturnForEditor(slot.edsave.returnPercentItterations);
		slot.edsave.returnTotalBet = (slot.edsave.returnPercentItterations * (slot.betsPerLine[slot.GetComponent<SlotComputeEditor>().betPerLineEditor].value * slot.GetComponent<SlotComputeEditor>().linesPlayedEditor));
		slot.edsave.returnPercent = ((float)slot.edsave.returnTotalWon / (float)slot.edsave.returnTotalBet);
	}
	void dumpToFile()
	{
		string fileName = SlotGlobals.MATH_PATH + "slot." + DateTime.Now.ToLongTimeString().Replace(":",".") + ".csv";
		if (File.Exists(fileName)) return;

		var sr = File.CreateText(fileName);
		sr.WriteLine (slot.edsave.returnData);
		sr.Close();
		slot.log ("Saved " + fileName + " to disk.");
	}
	void displayResultItem(ResultCount item)
	{
		float[] colWidth = {75,50,75,50,50,50};
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(item.matches.ToString() + " " + item.name, EditorStyles.miniTextField, GUILayout.Width(colWidth[0]));
		EditorGUILayout.LabelField(item.occurenceCount.ToString(), EditorStyles.miniTextField, GUILayout.Width(colWidth[1]));
		EditorGUILayout.LabelField(item.winTotal.ToString(), EditorStyles.miniTextField, GUILayout.Width(colWidth[2]));
		EditorGUILayout.LabelField(((float)item.winTotal / (float)slot.edsave.returnTotalWon).ToString ("F4"), EditorStyles.miniTextField, GUILayout.Width(colWidth[3]));
		EditorGUILayout.LabelField(((float)item.occurenceCount / slot.GetComponent<SlotComputeEditor>().itterations).ToString("F4"), EditorStyles.miniTextField, GUILayout.Width(colWidth[4]));
		EditorGUILayout.LabelField(((float)item.winTotal / (float)slot.edsave.returnTotalBet).ToString ("F4"), EditorStyles.miniTextField, GUILayout.Width(colWidth[5]));
		EditorGUILayout.EndHorizontal();
	}

	void foldoutReturn()
	{
		SlotComputeEditor compute = slot.GetComponent<SlotComputeEditor>();

		string est = "";
		if (compute.timeToComputeIterations > 0)
			est = "(est. " + ((slot.edsave.returnPercentItterations / (float)compute.timeToComputeIterations) * compute.estimatedTimeToCompute).ToString("F1") + "s)";
		slot.edsave.returnPercentItterations = EditorGUILayout.IntSlider(new GUIContent("Iterations " + est,SlotHelp.ITERATIONS), slot.edsave.returnPercentItterations,10,1000000);

		List<string> options = new List<string>();
		for (int i = 0; i < slot.betsPerLine.Count; i++) options.Add (slot.betsPerLine[i].value.ToString ());
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent("Bet Per Line", ""), GUILayout.MaxWidth(120));
		slot.GetComponent<SlotComputeEditor>().betPerLineEditor = EditorGUILayout.Popup(slot.GetComponent<SlotComputeEditor>().betPerLineEditor,options.ToArray(), GUILayout.MaxWidth(75));
		EditorGUILayout.EndHorizontal();
		slot.GetComponent<SlotComputeEditor>().linesPlayedEditor = EditorGUILayout.IntSlider(new GUIContent("Lines Played ", ""), slot.GetComponent<SlotComputeEditor>().linesPlayedEditor, 1, slot.lines.Count);

		slot.edsave.autoCompute = EditorGUILayout.Toggle(new GUIContent("Auto-Compute", SlotHelp.AUTOCOMPUTE), slot.edsave.autoCompute);

		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button(new GUIContent("Compute",SlotHelp.COMPUTE), GUILayout.MaxWidth(100))) {

			calcReturn ();

			slot.edsave.returnData = "";
			foreach(KeyValuePair<string, ResultCount>result in compute.resultCounts)
			{
				ResultCount item = result.Value;
				slot.edsave.returnData += item.matches + "," + 
					item.name + "," + 
					item.occurenceCount + "," + 
					item.winTotal + "," + 
						((float)item.winTotal / (float)slot.edsave.returnTotalWon).ToString ("F4") + "," + 
						((float)item.occurenceCount / (float)slot.edsave.returnPercentItterations).ToString("F4") + "," +
						((float)item.winTotal / (float)slot.edsave.returnTotalBet).ToString ("F4") + "\r\n";
			}
		}
		if (slot.GetComponent<SlotComputeEditor>().resultCounts.Count > 0)
		{
			if (GUILayout.Button(new GUIContent("Save CSV", SlotHelp.CSV), GUILayout.MaxWidth(100))) {
				dumpToFile();
			}
		}
		EditorGUILayout.EndHorizontal();

		if (slot.GetComponent<SlotComputeEditor>().resultCounts.Count > 0)
		{
			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Return Percentage", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("This slot sits at " + (slot.edsave.returnPercent * 100).ToString ("F2") + "% based on " + slot.GetComponent<SlotComputeEditor>().itterations + " spins.");
			if (slot.edsave.returnPercent <= 1)
			{
				EditorGUILayout.LabelField("This means on average, the house will take approximately " + (100 - Mathf.RoundToInt(slot.edsave.returnPercent * 100)) + " credits for every 100 credits bet.", EditorStyles.wordWrappedLabel);
			} else {
				EditorGUILayout.LabelField("This means on average, the house will pay out approximately " + Mathf.Abs(Mathf.RoundToInt(slot.edsave.returnPercent * 100)) + " credits for every 100 credits bet.", EditorStyles.wordWrappedLabel);
			}
			EditorGUILayout.EndVertical();

			float[] colWidth = {75,50,75,50,50,50};
			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Return Table based on " + slot.GetComponent<SlotComputeEditor>().itterations + " spins.", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("set", EditorStyles.miniBoldLabel, GUILayout.Width(colWidth[0]));
			EditorGUILayout.LabelField("hits", EditorStyles.miniBoldLabel, GUILayout.Width(colWidth[1]));
			EditorGUILayout.LabelField("paid", EditorStyles.miniBoldLabel, GUILayout.Width(colWidth[2]));
			EditorGUILayout.LabelField("%total", EditorStyles.miniBoldLabel, GUILayout.Width(colWidth[3]));
			EditorGUILayout.LabelField("prob%", EditorStyles.miniBoldLabel, GUILayout.Width(colWidth[4]));
			EditorGUILayout.LabelField("return%", EditorStyles.miniBoldLabel, GUILayout.Width(colWidth[5]));
			EditorGUILayout.EndHorizontal();

			//EditorGUILayout.BeginVertical();
			foreach(KeyValuePair<string, ResultCount>item in compute.resultCounts)
			{
				ResultCount res = (ResultCount)item.Value;
				displayResultItem(res);
			}

			EditorGUILayout.LabelField("Total bet: " + slot.GetComponent<SlotComputeEditor>().totalbet + " Total Won: " + slot.GetComponent<SlotComputeEditor>().totalwon, EditorStyles.boldLabel);

			EditorGUILayout.EndVertical();


			EditorGUILayout.BeginVertical("Box");
			EditorGUILayout.LabelField("Theoretical Volatility Breakdown based on " + slot.GetComponent<SlotComputeEditor>().itterations + " spins.", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("The volatility index of this slot sits at " +
			                           compute.volitility.ToString() + " with a standard deviation of " + compute.standardDeviation + ".", EditorStyles.wordWrappedLabel);
			EditorGUILayout.LabelField("Below is a list of lower and upper limit return percentages for a given number of spins:", EditorStyles.wordWrappedLabel);

			EditorGUI.indentLevel++;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("spins", EditorStyles.boldLabel, GUILayout.Width(100));
			EditorGUILayout.LabelField("lower", EditorStyles.boldLabel, GUILayout.Width(100));
			EditorGUILayout.LabelField("upper", EditorStyles.boldLabel, GUILayout.Width(100));
			EditorGUILayout.EndHorizontal();
			int hands = 10;
			for (int i = 0; i < 7; i++)
			{
				EditorGUILayout.BeginHorizontal();
				float low = (slot.edsave.returnPercent - compute.volitility / (Mathf.Sqrt(hands))) * 100;
				if (low < 0) low = 0;
				float high = (slot.edsave.returnPercent + compute.volitility / (Mathf.Sqrt(hands))) * 100;
				EditorGUILayout.LabelField(hands.ToString(), GUILayout.Width(100));
				EditorGUILayout.LabelField(low.ToString("F2") + " %", GUILayout.Width(100)); 
				EditorGUILayout.LabelField(high.ToString("F2") + " %", GUILayout.Width(100)); 
				hands = hands * 10;
				EditorGUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel--;

			//EditorGUILayout.TextField(new GUIContent("Varience"), compute.variance.ToString());
			//EditorGUILayout.TextField(new GUIContent("Volatility"), compute.volitility.ToString());

			EditorGUILayout.EndVertical();
		}
	}
	#endregion


	#region Basic Settings

	void showAddRemoveReelCount()
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent("Reel Count", SlotHelp.NUMBER_OF_REELS), GUILayout.MaxWidth(100));
		GUILayout.FlexibleSpace();

		if (slot.numberOfReels > 1)
			if (GUILayout.Button("-",EditorStyles.miniButton, GUILayout.Width(30))) {
			
			for (int i = 0; i < slot.symbolSetNames.Count; i++)
			{
				slot.setPays[i].pays.RemoveAt (slot.setPays[i].pays.Count - 1);
				slot.setPays[i].anticipate.RemoveAt (slot.setPays[i].anticipate.Count - 1);
			}
			for (int i = 0; i < slot.lines.Count; i++)
			{
				slot.lines[i].positions.RemoveAt (slot.lines[i].positions.Count - 1);
			}
			
			slot.numberOfReels--;
		}

		EditorGUILayout.LabelField(new GUIContent(slot.numberOfReels.ToString(), ""), EditorStyles.boldLabel, GUILayout.Width(50));

		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.numberOfReels++;
			for (int i = 0; i < slot.symbolSetNames.Count; i++)
			{
				slot.setPays[i].pays.Add (0);
				slot.setPays[i].anticipate.Add (false);
			}
			for (int i = 0; i < slot.lines.Count; i++)
			{
				slot.lines[i].positions.Add (0);
			}
		}

		EditorGUILayout.EndHorizontal();
	}

	void foldoutBasicSettings()
	{
		EditorGUILayout.LabelField(new GUIContent("Reel"), EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.BeginVertical("Box");
		GUILayout.Space (5);
		showAddRemoveReelCount();
		slot.reelHeight = EditorGUILayout.IntSlider(new GUIContent("Symbol Height", SlotHelp.SYMBOL_HEIGHT), slot.reelHeight,3,20);
		slot.reelIndent = EditorGUILayout.IntSlider(new GUIContent("Vertical Indent", SlotHelp.VERTICAL_INDENT), slot.reelIndent,0,Mathf.RoundToInt((slot.reelHeight-1)/2));
		slot.reelBackground = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Symbol Bkg Prefab", SlotHelp.BACKGROUND_PREFAB), slot.reelBackground, typeof(GameObject), false);
		EditorGUI.indentLevel--;
		GUILayout.Space (5);
		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField(new GUIContent("Bet"), EditorStyles.boldLabel); 
		EditorGUI.indentLevel++;
		EditorGUILayout.BeginVertical("Box");
		GUILayout.Space (5);
//		slot.GetComponent<SlotCredits>().maxBetPerLine = EditorGUILayout.IntSlider(new GUIContent("Max Bet Per Line", SlotHelp.MAX_BET_PER_LINE), slot.GetComponent<SlotCredits>().maxBetPerLine,1,20);

		EditorGUILayout.BeginHorizontal();
		
		List<string> options = new List<string>();
		for (int i = 0; i < slot.betsPerLine.Count; i++) options.Add (slot.betsPerLine[i].value.ToString ());
		EditorGUILayout.LabelField(new GUIContent("Initial Bet Per Line", SlotHelp.INITIAL_BET_PER_LINE), GUILayout.MaxWidth(100));
		slot.GetComponent<SlotCredits>().betPerLineDefaultIndex = EditorGUILayout.Popup(slot.GetComponent<SlotCredits>().betPerLineDefaultIndex,options.ToArray(), GUILayout.MaxWidth(75));
		//		slot.GetComponent<SlotCredits>().betPerLineIndex = EditorGUILayout.IntSlider(new GUIContent("Initial Bet Per Line", SlotHelp.INITIAL_BET_PER_LINE), slot.GetComponent<SlotCredits>().betPerLine,1,slot.GetComponent<SlotCredits>().maxBetPerLine);
		EditorGUILayout.EndHorizontal();

		//GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		//EditorGUI.indentLevel++;
		EditorGUILayout.LabelField(new GUIContent("Valid Bets", SlotHelp.VALID_BETS), GUILayout.MaxWidth(100));
		//EditorGUI.indentLevel--;
		
		for (int c = 0; c < slot.betsPerLine.Count; c++)
		{
			EditorGUI.indentLevel--;
			EditorGUILayout.LabelField(new GUIContent((c + 1).ToString()), EditorStyles.miniLabel, GUILayout.MaxWidth(20));
			
			slot.betsPerLine[c].value = EditorGUILayout.IntField(slot.betsPerLine[c].value, GUILayout.Width(25));
			slot.betsPerLine[c].canBet = EditorGUILayout.Toggle(slot.betsPerLine[c].canBet, GUILayout.Width(25));
			if (((c+1) % 3) == 0)
			{
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(100));
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel++;
			
		}

		if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30))) {
			
			slot.betsPerLine.RemoveAt(slot.betsPerLine.Count-1);
			slot.betsPerLine.Capacity = slot.betsPerLine.Capacity - 1;

		}

		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.betsPerLine.Capacity = slot.betsPerLine.Capacity + 1;
			slot.betsPerLine.Add(new BetsWrapper()); 
		}
		EditorGUILayout.EndHorizontal();

		slot.GetComponent<SlotCredits>().linesPlayed = EditorGUILayout.IntSlider(new GUIContent("Initial Lines Played", SlotHelp.INITIAL_LINES_PLAYED), slot.GetComponent<SlotCredits>().linesPlayed,1,slot.lines.Count);
		slot.GetComponent<SlotCredits>().persistant = EditorGUILayout.Toggle(new GUIContent("Persist",SlotHelp.PERSIST), slot.GetComponent<SlotCredits>().persistant);
		EditorGUI.indentLevel--;
		GUILayout.Space (5);
		EditorGUILayout.EndVertical();


		EditorGUILayout.LabelField(new GUIContent("Reel Timings", SlotHelp.REEL_TIMING), EditorStyles.boldLabel);
		EditorGUI.indentLevel++;
		EditorGUILayout.BeginVertical("Box");
		GUILayout.Space (5);
		slot.spinTime = EditorGUILayout.Slider (new GUIContent("Initial Reel Stop", SlotHelp.INITIAL_REEL_STOP_TIME), slot.spinTime, 0.1f, 5.0f);
		slot.spinTimeIncPerReel = EditorGUILayout.Slider (new GUIContent("Additional Stops", SlotHelp.STOP_TIME_EACH_REEL), slot.spinTimeIncPerReel, 0.1f, 5.0f);
		slot.spinningSpeed = EditorGUILayout.Slider(new GUIContent("Spin Speed", SlotHelp.SPIN_SPEED), slot.spinningSpeed * 10,0.033f,2.0f) / 10.0f;
		slot.easeOutTime = EditorGUILayout.Slider(new GUIContent("Ease Time To Stop", SlotHelp.EASE_TIME), slot.easeOutTime,0.0f,slot.spinTimeIncPerReel * 0.8f);
		slot.reelEase = (EaseType)EditorGUILayout.EnumPopup(new GUIContent("Ease Type", SlotHelp.EASE_TYPE), slot.reelEase);
		slot.GetComponent<SlotWins>().showWinTime = EditorGUILayout.Slider(new GUIContent("Win Display Time", SlotHelp.WIN_DISPLAY_TIME), slot.GetComponent<SlotWins>().showWinTime,0.25f,5.0f);
		slot.GetComponent<SlotWins>().delayBetweenShowingWins = EditorGUILayout.Slider(new GUIContent("Delay Between Wins", SlotHelp.TIMEOUT_BETWEEN_WINS), slot.GetComponent<SlotWins>().delayBetweenShowingWins,0.0f,2.0f);
		EditorGUI.indentLevel--;
		GUILayout.Space (5);
		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField(new GUIContent("Pay Lines", SlotHelp.REEL_TIMING), EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical("Box");
		GUILayout.Space (5);
		EditorGUI.indentLevel++;
		slot.GetComponent<SlotLines>().linesEnabled = EditorGUILayout.Toggle(new GUIContent("Line Drawing", SlotHelp.LINE_DRAWING), slot.GetComponent<SlotLines>().linesEnabled);
		slot.GetComponent<SlotLines>().linesZorder = EditorGUILayout.Slider(new GUIContent("Z-Order", SlotHelp.LINE_ZORDER), slot.GetComponent<SlotLines>().linesZorder,-100.0f,100.0f);
		if (slot.GetComponent<SlotLines>().linesShader == null)
		{
			slot.GetComponent<SlotLines>().linesShader = Shader.Find ("Mobile/Particles/Alpha Blended");
		}
		slot.GetComponent<SlotLines>().linesShader = (Shader)EditorGUILayout.ObjectField(new GUIContent("Shader", SlotHelp.LINE_ZORDER), slot.GetComponent<SlotLines>().linesShader, typeof(Shader), false);
		slot.GetComponent<SlotLines>().payLineWidth = EditorGUILayout.Slider(new GUIContent("Line Width", SlotHelp.PAYLINE_WIDTH), slot.GetComponent<SlotLines>().payLineWidth,0.01f,100.0f);
		slot.GetComponent<SlotLines>().payLineColor1 = EditorGUILayout.ColorField(new GUIContent("Payline Color 1", SlotHelp.PAYLINE_COLOR), slot.GetComponent<SlotLines>().payLineColor1);
		slot.GetComponent<SlotLines>().payLineColor2 = EditorGUILayout.ColorField(new GUIContent("Payline Color 2", SlotHelp.PAYLINE_COLOR), slot.GetComponent<SlotLines>().payLineColor2);
		slot.GetComponent<SlotLines>().strokeWidth = EditorGUILayout.Slider(new GUIContent("Stroke Width", SlotHelp.PAYLINE_STROKE_WIDTH), slot.GetComponent<SlotLines>().strokeWidth,0.0f,100.0f);
		slot.GetComponent<SlotLines>().strokeColor = EditorGUILayout.ColorField(new GUIContent("Stroke Color", SlotHelp.PAYLINE_STROKE_COLOR), slot.GetComponent<SlotLines>().strokeColor);
		EditorGUI.indentLevel--;
		GUILayout.Space (5);
		EditorGUILayout.EndVertical();

		EditorGUILayout.LabelField(new GUIContent("Misc"), EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical("Box");
		GUILayout.Space (5);
		EditorGUI.indentLevel++;
		if (slot.callbackScriptName == "") 
		{
			callbackScript = (MonoScript)EditorGUILayout.ObjectField(new GUIContent("Callback Script", SlotHelp.CALLBACKS), callbackScript, typeof(MonoScript), true);
			if (callbackScript != null)
				slot.callbackScriptName = callbackScript.name;
		} else {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Callback Script");

			if (GUILayout.Button(slot.callbackScriptName + " (MonoScript) Clear"))
			{
				callbackScript = null;
				slot.callbackScriptName = "";
			}
			EditorGUILayout.EndHorizontal();
		}
		slot.activeRNG = (pRNGs)EditorGUILayout.EnumPopup(new GUIContent("Active RNG", SlotHelp.ACTIVE_RNG), slot.activeRNG);
		slot.debugMode = EditorGUILayout.Toggle(new GUIContent("Debug Mode", "Enable or disable all Slot related debugging messages"), slot.debugMode);
		slot.usePool = EditorGUILayout.Toggle(new GUIContent("Use Prefab Pool", SlotHelp.USE_PREFAB_POOL), slot.usePool);
		EditorGUI.indentLevel--;
		GUILayout.Space (5);
		EditorGUILayout.EndVertical();

	}
	#endregion

	#region Symbol Setup
	void showUpDownSymbol(int index)
	{
		if (index > 0)
		if (GUILayout.Button("", EditorStyles.miniButton, GUILayout.Width(30))) {

			GameObject temp = slot.symbolPrefabs[index];
			slot.symbolPrefabs[index] = slot.symbolPrefabs[index-1];
			slot.symbolPrefabs[index-1] = temp;

			temp = slot.winboxPrefabs[index];
			slot.winboxPrefabs[index] = slot.winboxPrefabs[index-1];
			slot.winboxPrefabs[index-1] = temp;

			FrequencyWrapper temp2 = slot.reelFrequencies[index];
			slot.reelFrequencies[index] = slot.reelFrequencies[index-1];
			slot.reelFrequencies[index-1] = temp2;

			int temp3 = slot.symbolFrequencies[index];
			slot.symbolFrequencies[index] = slot.symbolFrequencies[index-1];
			slot.symbolFrequencies[index-1] = temp3;
		}
		if (index < slot.symbolPrefabs.Capacity - 1)
		if (GUILayout.Button("v", EditorStyles.miniButton, GUILayout.Width(30))) {

			GameObject temp = slot.symbolPrefabs[index];
			slot.symbolPrefabs[index] = slot.symbolPrefabs[index+1];
			slot.symbolPrefabs[index+1] = temp;
			
			temp = slot.winboxPrefabs[index];
			slot.winboxPrefabs[index] = slot.winboxPrefabs[index+1];
			slot.winboxPrefabs[index+1] = temp;
			
			FrequencyWrapper temp2 = slot.reelFrequencies[index];
			slot.reelFrequencies[index] = slot.reelFrequencies[index+1];
			slot.reelFrequencies[index+1] = temp2;
			
			int temp3 = slot.symbolFrequencies[index];
			slot.symbolFrequencies[index] = slot.symbolFrequencies[index+1];
			slot.symbolFrequencies[index+1] = temp3;
		}
	}
	void showAddRemoveSymbol()
	{
		EditorGUILayout.BeginHorizontal();
		if (slot.symbolPrefabs.Count > 0)
		if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30))) {
			
			slot.symbolPrefabs.RemoveAt(slot.symbolPrefabs.Count-1);
			slot.symbolPrefabs.Capacity = slot.symbolPrefabs.Capacity - 1;
			slot.winboxPrefabs.RemoveAt(slot.winboxPrefabs.Count-1);
			slot.winboxPrefabs.Capacity = slot.winboxPrefabs.Capacity - 1;

			while (slot.reelFrequencies.Count > slot.symbolPrefabs.Count)
			{
				slot.reelFrequencies.RemoveAt(slot.reelFrequencies.Count-1);
				slot.reelFrequencies.Capacity = slot.reelFrequencies.Capacity - 1;
			}

			while (slot.symbolFrequencies.Count > slot.symbolPrefabs.Count)
			{
				slot.symbolFrequencies.RemoveAt(slot.symbolFrequencies.Count-1);
				slot.symbolFrequencies.Capacity = slot.symbolFrequencies.Capacity - 1;
			}

		}
		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.symbolPrefabs.Capacity = slot.symbolPrefabs.Capacity + 1;
			slot.symbolPrefabs.Add(null); 
			slot.winboxPrefabs.Capacity = slot.winboxPrefabs.Capacity + 1;
			if (slot.winboxPrefabs.Count > 0)
			{
				slot.winboxPrefabs.Add(slot.winboxPrefabs[slot.winboxPrefabs.Count-1]); 
			} else {
				slot.winboxPrefabs.Add(null); 
			}

			slot.reelFrequencies.Capacity = slot.reelFrequencies.Capacity + 1;
			slot.reelFrequencies.Add (new FrequencyWrapper(slot.numberOfReels));

			slot.symbolFrequencies.Capacity = slot.symbolFrequencies.Capacity + 1;
			slot.symbolFrequencies.Add (1);
		}
		EditorGUILayout.EndHorizontal();
	}
	void foldoutSymbolSetup()
	{
		if (slot.symbolPrefabs.Count > 0)
		{
			slot.percision = EditorGUILayout.IntSlider(new GUIContent("Global Precision", SlotHelp.SYMBOL_PRECISION), slot.percision, 5, 100);
		}

		for (int i = 0; i < slot.symbolPrefabs.Count; i++)
		{
			EditorGUILayout.BeginVertical("Box");
			string lname = (slot.symbolPrefabs[i] != null)?(i+1).ToString () + " - " + slot.symbolPrefabs[i].name:(i+1).ToString() + " - None";
			EditorGUILayout.LabelField(new GUIContent(lname, SlotHelp.SYMBOL_PROPERTIES), EditorStyles.boldLabel, GUILayout.MaxWidth(100));

			EditorGUI.indentLevel++;
			slot.symbolPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab"), slot.symbolPrefabs[i], typeof(GameObject), false);

			if (slot.symbolPrefabs[i] != null)
			{
				//EditorGUI.DrawPreviewTexture(new Rect(0,0,25,25),slot.symbolPrefabs[i].GetComponent<SpriteRenderer>().sprite.texture.);

				SlotSymbol currentSymbol = slot.symbolPrefabs[i].GetComponent<SlotSymbol>();
				EditorGUILayout.BeginHorizontal();
				slot.winboxPrefabs[i] = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Winbox"), slot.winboxPrefabs[i], typeof(GameObject), false);
				currentSymbol.isWild = EditorGUILayout.ToggleLeft(new GUIContent("Wild"),currentSymbol.isWild, EditorStyles.miniLabel, GUILayout.Width(75));
				EditorGUILayout.EndHorizontal();

				//Clamping
				if ((slot.symbolPrefabs[i] != null) && (slot.symbolPrefabs[i].GetComponent<SlotSymbol>() != null))
				{
					List<string> optionsCap = new List<string>();
					
					optionsCap.Add ("off");
					for (int c = 1; c <= (slot.reelHeight - (slot.reelIndent * 2)); c++) optionsCap.Add (c.ToString ());
					
					EditorGUILayout.BeginHorizontal();
					//EditorGUI.indentLevel++;
					EditorGUILayout.LabelField(new GUIContent("Clamping",SlotHelp.SYMBOL_CLAMP), GUILayout.MaxWidth(115));
					
					
					EditorGUI.indentLevel--;
					//EditorGUI.indentLevel--;
					GUILayout.FlexibleSpace();
					currentSymbol.clampPerReel = EditorGUILayout.Popup(currentSymbol.clampPerReel, optionsCap.ToArray(), GUILayout.Width(30));
					EditorGUILayout.LabelField("max per reel", EditorStyles.miniLabel, GUILayout.Width(70));
					
					if (currentSymbol.clampPerReel > 0)
					{
						optionsCap.Clear ();
						optionsCap.Add ("off");
						
						for (int c = 1; c <= (currentSymbol.clampPerReel * slot.numberOfReels); c++) optionsCap.Add (c.ToString ());
						//EditorGUIUtility.labelWidth = 50;
						//EditorGUILayout.LabelField(new GUIContent("total",""), GUILayout.MaxWidth(50));
						currentSymbol.clampTotal = EditorGUILayout.Popup(currentSymbol.clampTotal, optionsCap.ToArray(), GUILayout.Width(30));
						EditorGUILayout.LabelField("total all reels", EditorStyles.miniLabel, GUILayout.Width(70));
						//EditorGUIUtility.labelWidth = 0;
					}
					EditorGUI.indentLevel++;
					//EditorGUI.indentLevel++;
					EditorGUILayout.EndHorizontal();
				}

				if (!currentSymbol.perReelFrequency)
				{
					slot.symbolFrequencies[i] = EditorGUILayout.IntSlider(new GUIContent("Frequency", SlotHelp.SYMBOL_OCCURENCE), slot.symbolFrequencies[i], 0, slot.percision);
					EditorGUILayout.BeginHorizontal();
					EditorGUI.indentLevel++;
					currentSymbol.perReelFrequency = EditorGUILayout.ToggleLeft(new GUIContent("Enable Frequency Per Reel"),currentSymbol.perReelFrequency);
					EditorGUI.indentLevel--;
				} else {
					EditorGUILayout.BeginHorizontal();
					while (slot.reelFrequencies.Count < slot.symbolPrefabs.Count)
					{
						slot.reelFrequencies.Capacity += 1;
						slot.reelFrequencies.Add (new FrequencyWrapper(slot.numberOfReels));
					}
					while (slot.reelFrequencies[i].freq.Count < slot.numberOfReels)
					{
						slot.reelFrequencies[i].freq.Capacity += 1;
						slot.reelFrequencies[i].freq.Add (0);
					}
					currentSymbol.perReelFrequency = EditorGUILayout.ToggleLeft(new GUIContent("Frequency Per Reel"),currentSymbol.perReelFrequency);
				}

				EditorGUILayout.EndHorizontal();

				if (currentSymbol.perReelFrequency)
				{
					EditorGUI.indentLevel++;
					for (int reelFreq = 0; reelFreq < slot.numberOfReels; reelFreq++)
					{
						slot.reelFrequencies[i].freq[reelFreq] = EditorGUILayout.IntSlider(new GUIContent("Reel " + (reelFreq + 1).ToString(), SlotHelp.SYMBOL_OCCURENCE), slot.reelFrequencies[i].freq[reelFreq], 0, slot.percision);
					}
					EditorGUI.indentLevel--;
				}
			}
			//showUpDownSymbol(i);
			EditorGUI.indentLevel--;
			EditorGUILayout.EndVertical();
		}
		showAddRemoveSymbol();

	}
	#endregion

	#region Symbol Sets Setup
	List<string> getSymbolList()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < slot.symbolPrefabs.Count; i++)
		{
			if (slot.symbolPrefabs[i] != null)
			list.Add (slot.symbolPrefabs[i].name);
		}
		return list;
	}

	void showSymbolSetFoldout(int index)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField(new GUIContent("Name:", SlotHelp.SET_NAME), GUILayout.MaxWidth(100));
		EditorGUI.indentLevel--;
		slot.symbolSetNames[index] = EditorGUILayout.TextField(slot.symbolSetNames[index], GUILayout.MinWidth(50));
		EditorGUILayout.EndHorizontal();

		if (slot.symbolSetNames[index] == "") return;
		EditorGUILayout.BeginHorizontal();
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField(new GUIContent("Set Type:", SlotHelp.SET_TYPE), GUILayout.MaxWidth(100));
		EditorGUI.indentLevel--;
		slot.symbolSets[index].typeofSet = (SetsType)EditorGUILayout.EnumPopup(slot.symbolSets[index].typeofSet, GUILayout.MaxWidth(75));
		slot.symbolSets[index].allowWilds = EditorGUILayout.ToggleLeft("Allow Wilds", slot.symbolSets[index].allowWilds);
		EditorGUILayout.EndHorizontal();

		if (slot.symbolSets[index].typeofSet == SetsType.scatter)
		{
			while (slot.symbolSets[index].symbols.Count > 1)
			{
				slot.symbolSets[index].symbols.RemoveAt (slot.symbolSets[index].symbols.Count-1);
			}
			int max = slot.symbolPrefabs[slot.symbolSets[index].symbols[0]].GetComponent<SlotSymbol>().clampPerReel * slot.numberOfReels;
			int maxtotal = slot.symbolPrefabs[slot.symbolSets[index].symbols[0]].GetComponent<SlotSymbol>().clampTotal;
			if (maxtotal > 0)
				if (maxtotal < max) max = maxtotal;
				
			while (slot.setPays[index].anticipate.Count > (max))
			{
				slot.setPays[index].anticipate.RemoveAt (slot.setPays[index].anticipate.Count-1);
			}
			while (slot.setPays[index].anticipate.Count < (max))
			{
				slot.setPays[index].anticipate.Add (false);
			}
			while (slot.setPays[index].pays.Count > (max))
			{
				slot.setPays[index].pays.RemoveAt (slot.setPays[index].pays.Count-1);
			}
			while (slot.setPays[index].pays.Count < (max))
			{
				slot.setPays[index].pays.Add (0);
			}
		} else {
			while (slot.setPays[index].anticipate.Count > (slot.numberOfReels))
			{
				slot.setPays[index].anticipate.RemoveAt (slot.setPays[index].anticipate.Count-1);
			}
			while (slot.setPays[index].anticipate.Count < (slot.numberOfReels))
			{
				slot.setPays[index].anticipate.Add (false);
			}
			while (slot.setPays[index].pays.Count > (slot.numberOfReels))
			{
				slot.setPays[index].pays.RemoveAt (slot.setPays[index].pays.Count-1);
			}
			while (slot.setPays[index].pays.Count < (slot.numberOfReels))
			{
				slot.setPays[index].pays.Add (0);
			}
		}

		EditorGUILayout.BeginHorizontal();
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField(new GUIContent("Symbols:", SlotHelp.SET_SYMBOLS), GUILayout.MaxWidth(100));
		EditorGUI.indentLevel--;
		for (int c = 0; c < slot.symbolSets[index].symbols.Count; c++)
		{
			slot.symbolSets[index].symbols[c] =  EditorGUILayout.Popup(slot.symbolSets[index].symbols[c],getSymbolList().ToArray(), GUILayout.MaxWidth(75));
			//EditorGUILayout.IntField(slot.winningSymbolSets[index].symbols[c], GUILayout.MaxWidth(30));
		}
		
		if (slot.symbolSets[index].symbols.Count > 1)
		if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.symbolSets[index].symbols.RemoveAt(slot.symbolSets[index].symbols.Count-1);
			slot.symbolSets[index].symbols.Capacity = slot.symbolSets[index].symbols.Capacity - 1;
		}
		if (slot.symbolSets[index].typeofSet != SetsType.scatter)
		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.symbolSets[index].symbols.Add (0);
			//				slot.winningSymbolSets[index].symbols.Capacity = slot.winningSymbolSets.Capacity - 1;
		}
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.indentLevel++;
		EditorGUILayout.LabelField(new GUIContent("Match Pays", SlotHelp.SET_PAYS), GUILayout.MaxWidth(100));
		EditorGUI.indentLevel--;

		if (slot.symbolSets[index].typeofSet == SetsType.scatter)
		{
			if (slot.setPays[index].pays.Count == 0)
			{
				GUI.color = Color.red;
				EditorGUILayout.LabelField("Error. See notification area for info.", EditorStyles.whiteLabel);
				GUI.color = Color.green;
			}
		}

		bool mark = false;
		for (int c = 0; c < slot.setPays[index].anticipate.Count; c++)
		{
			if (mark)
			{
				slot.setPays[index].anticipate[c] = true;
			} else
			if (slot.setPays[index].anticipate[c] == true)
			{
				mark = true;
			}
		}

		for (int c = 0; c < slot.setPays[index].pays.Count; c++)
		{
			EditorGUI.indentLevel--;

			GUI.color = new Color(0.4f,0.4f,0.4f,1.0f);
			EditorGUILayout.LabelField(new GUIContent((c + 1).ToString()), EditorStyles.whiteMiniLabel, GUILayout.MaxWidth(20));
			GUI.color = Color.green;
			slot.setPays[index].pays[c] = EditorGUILayout.IntField(slot.setPays[index].pays[c], GUILayout.Width(50));
			//EditorGUILayout.LabelField(new GUIContent("Ant"), EditorStyles.miniLabel, GUILayout.MaxWidth(30));
			if ((c < slot.setPays[index].anticipate.Count-1) && (slot.symbolSets[index].typeofSet == SetsType.scatter))
			{
				slot.setPays[index].anticipate[c] = EditorGUILayout.ToggleLeft(new GUIContent("Ant", SlotHelp.SET_ANTICIPATION), slot.setPays[index].anticipate[c], GUILayout.Width(40));
			}

			if (((c+1) % ((slot.symbolSets[index].typeofSet == SetsType.scatter)?2:4)) == 0)
			{
				EditorGUILayout.EndHorizontal();
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("", GUILayout.MaxWidth(100));
				EditorGUI.indentLevel--;
			}
			EditorGUI.indentLevel++;

		}
		EditorGUILayout.EndHorizontal();
	}

	void showUpDownSymbolSet(int index)
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (index > 0)
		if (GUILayout.Button("up", EditorStyles.miniButton, GUILayout.Width(50))) {
			
			string temp = slot.symbolSetNames[index];
			slot.symbolSetNames[index] = slot.symbolSetNames[index-1];
			slot.symbolSetNames[index-1] = temp;
			
			SetsWrapper temp2 = slot.symbolSets[index];
			slot.symbolSets[index] = slot.symbolSets[index-1];
			slot.symbolSets[index-1] = temp2;
			
			PaysWrapper temp3 = slot.setPays[index];
			slot.setPays[index] = slot.setPays[index-1];
			slot.setPays[index-1] = temp3;
		}
		if (index < slot.symbolSetNames.Capacity - 1)
		if (GUILayout.Button("down", EditorStyles.miniButton, GUILayout.Width(50))) {
			
			string temp = slot.symbolSetNames[index];
			slot.symbolSetNames[index] = slot.symbolSetNames[index+1];
			slot.symbolSetNames[index+1] = temp;
			
			SetsWrapper temp2 = slot.symbolSets[index];
			slot.symbolSets[index] = slot.symbolSets[index+1];
			slot.symbolSets[index+1] = temp2;

			PaysWrapper temp3 = slot.setPays[index];
			slot.setPays[index] = slot.setPays[index+1];
			slot.setPays[index+1] = temp3;
		}
		EditorGUILayout.EndHorizontal();
	}
	void showAddRemoveSymbolSet()
	{
		EditorGUILayout.BeginHorizontal();
		if (slot.symbolSetNames.Count > 0)
		if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.symbolSetNames.RemoveAt(slot.symbolSetNames.Count-1);
			slot.symbolSetNames.Capacity = slot.symbolSetNames.Capacity - 1;
			
			slot.symbolSets.RemoveAt(slot.symbolSets.Count-1);
			slot.symbolSets.Capacity = slot.symbolSets.Capacity - 1;
			
			slot.setPays.RemoveAt(slot.setPays.Count-1);
			slot.setPays.Capacity = slot.setPays.Capacity - 1;

			setFoldouts.RemoveAt(setFoldouts.Count-1);
		}
		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			
			slot.symbolSetNames.Capacity = slot.symbolSetNames.Capacity + 1;
			slot.symbolSetNames.Add(""); 
			
			//slot.winningSymbolSets.Capacity = slot.winningSymbolSets.Capacity + 1;
			//slot.winningSymbolSets.Add(null); 
			
			slot.symbolSets.Capacity = slot.symbolSets.Capacity + 1;
			slot.symbolSets.Add (new SetsWrapper());
			
			SetsWrapper setsets = slot.symbolSets[slot.symbolSets.Capacity - 1];
			setsets.symbols = new List<int>();
			setsets.symbols.Add (0);
			
			slot.setPays.Capacity = slot.setPays.Capacity + 1;
			slot.setPays.Add (new PaysWrapper());
			
			PaysWrapper setpays = slot.setPays[slot.setPays.Capacity - 1];
			setpays.pays = new List<int>();
			setpays.pays.Capacity = slot.numberOfReels;
			setpays.anticipate = new List<bool>();
			setpays.anticipate.Capacity = slot.numberOfReels;
			for (int c = 0; c < slot.numberOfReels; c++)
			{
				slot.setPays[slot.setPays.Capacity-1].pays.Add (0);
				slot.setPays[slot.setPays.Capacity-1].anticipate.Add (false);
			}
			setFoldouts.Add (true);
		}
		EditorGUILayout.EndHorizontal();
	}

	void foldoutSymbolSetsSetup()
	{
		for (int i = 0; i < slot.symbolSetNames.Count; i++)
		{
			EditorGUILayout.BeginVertical("Box");
			EditorGUI.indentLevel++;
			string setName = (slot.symbolSetNames[i] == "")?"undefined":slot.symbolSetNames[i];
			setFoldouts[i] = EditorGUILayout.Foldout(setFoldouts[i], new GUIContent((i+1).ToString() + " - " + setName, ""), EditorStyles.foldout);
			EditorGUI.indentLevel--;
			EditorGUI.indentLevel++;
			if (setFoldouts[i])
			{
				showSymbolSetFoldout(i);
			}
			EditorGUI.indentLevel--;
			showUpDownSymbolSet(i);
			EditorGUILayout.EndVertical();
		}

		showAddRemoveSymbolSet();
	}
	#endregion

	#region Line Setup
	void showAddRemoveLine()
	{
		EditorGUILayout.BeginHorizontal();
		if (slot.lines.Count > 0)
		if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30))) {
			slot.lines.RemoveAt(slot.lines.Count-1);
			slot.lines.Capacity = slot.lines.Capacity - 1;
			
		}
		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			
			slot.lines.Capacity = slot.lines.Capacity + 1;
			slot.lines.Add (new LinesWrapper());
			
			LinesWrapper setlines = slot.lines[slot.lines.Capacity - 1];
			setlines.positions = new List<int>();
			for (int c = 0; c < slot.numberOfReels; c++)
			{
				slot.lines[slot.lines.Capacity-1].positions.Add (0);
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	void showIncAllButtons(int line)
	{
		if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(30))) {

			for (int c = 0; c < slot.lines[line].positions.Count; c++)
			{
				slot.lines[line].positions[c]--;
			}
		}
		if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(30))) {
			
			for (int c = 0; c < slot.lines[line].positions.Count; c++)
			{
				slot.lines[line].positions[c]++;
			}
		}
	}
	void foldoutLinesSetup()
	{
		List<string> options = new List<string>();

		for (int i = slot.reelIndent; i < (slot.reelHeight - slot.reelIndent); i++) options.Add (i.ToString ());

		for (int i = 0; i < slot.lines.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Line #" + (i + 1).ToString(), SlotHelp.PAYLINES), GUILayout.Width(65));

			for (int c = 0; c < slot.lines[i].positions.Count; c++)
			{
				if (slot.lines[i].positions[c] < slot.reelIndent) slot.lines[i].positions[c] = slot.reelIndent; else
					if (slot.lines[i].positions[c] > ((slot.reelHeight - 1) - slot.reelIndent)) slot.lines[i].positions[c] = ((slot.reelHeight - 1) - slot.reelIndent);
				int result =  EditorGUILayout.Popup(slot.lines[i].positions[c] - slot.reelIndent,options.ToArray(), GUILayout.MaxWidth(30));
				slot.lines[i].positions[c] = int.Parse(options[result]);
				//EditorGUILayout.IntField(slot.winningSymbolSets[i].symbols[c], GUILayout.MaxWidth(30));
				if (((c+1) % 8) == 0)
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(new GUIContent(""), GUILayout.Width(65));
				}
			}
			showIncAllButtons(i);
			EditorGUILayout.EndHorizontal();
		}

		showAddRemoveLine();
	}
	#endregion
	
	[MenuItem ("GameObject/Slots Creator/Create Slot Machine")]
	static void MakeSlot()
	{
		GameObject go = new GameObject("Slot");
		go.AddComponent(typeof(Slot));
	}
	[MenuItem ("GameObject/Slots Creator/Create Slot With Simple GUI")]
	static void MakeSlotGUI()
	{
		GameObject go = new GameObject("Slot");
		go.AddComponent(typeof(Slot));
		go.AddComponent(typeof(SlotSimpleGUI));
	}
	[MenuItem ("GameObject/Slots Creator/Create 2D Sprite Symbol")]
	static void Make2DSymbol()
	{
		GameObject go = new GameObject("Symbol");
		go.AddComponent(typeof(SlotSymbol));
		go.AddComponent(typeof(SpriteRenderer));
	}
	[MenuItem ("GameObject/Slots Creator/Create 3D Mesh Symbol")]
	static void Make3DSymbol()
	{
		GameObject go = new GameObject("Symbol");
		go.AddComponent(typeof(SlotSymbol));
		go.AddComponent(typeof(MeshFilter));
		go.AddComponent(typeof(MeshRenderer));
	}

	void drawNotifications()
	{
		if (configIssues.Count == 0)
		{
			EditorGUILayout.HelpBox("\n\rNo errors found in setup.\n\r", MessageType.Info);
		} else {
			
			string errorMsgs = "\n\r";
			foreach(string issue in configIssues)
			{
				errorMsgs += issue + "\n\r";
			}
			EditorGUILayout.HelpBox(errorMsgs, MessageType.Warning);
		}
	}
	void checkErrors()
	{
		if (slot.betsPerLine.Count == 0)  
		{
			slot.betsPerLine.Add (new BetsWrapper());
			slot.betsPerLine[0].value = 1;
			slot.betsPerLine[0].canBet = true;
		}
		//if (Application.isPlaying) return;
		Vector3 size = Vector3.zero;

		if (slot.symbolPrefabs.Count == 0)
		{
			configIssues.Add ("Add a Symbol under Symbol Setup.");
		}
		if (slot.symbolSets.Count == 0)
		{
			configIssues.Add ("Add a Symbol Set under Symbol Sets Setup.");
		}
		if (slot.lines.Count == 0)
		{
			configIssues.Add ("Add a Line under Payline Setup.");
		}
		for (int symbolIndex = 0; symbolIndex < slot.symbolPrefabs.Count; symbolIndex++)
		{

			if (slot.symbolPrefabs[symbolIndex] == null) { configIssues.Add ("Assign a prefab Game Object to symbol #" + (symbolIndex + 1) + "."); continue; }
			if (slot.symbolPrefabs[symbolIndex].GetComponent<SlotSymbol>() == null) { configIssues.Add ("Symbol #" + (symbolIndex + 1) + " is invalid (missing the SlotSymbol component)"); continue; }
			if ((slot.symbolPrefabs[symbolIndex].GetComponent<SpriteRenderer>() == null) &&
			    (slot.symbolPrefabs[symbolIndex].GetComponent<MeshFilter>() == null)) { configIssues.Add ("Symbol #" + (symbolIndex + 1) + " is an invalid (missing SpriteRenderer or MeshFilter)"); continue; }

			SlotSymbol symbol = slot.symbolPrefabs[symbolIndex].GetComponent<SlotSymbol>();

			if (symbol.perReelFrequency)
			{
				int totalOccurenceReel = 0;
				foreach(int freq in slot.reelFrequencies[symbolIndex].freq)
				{
					//totalOccurence += freq;
					totalOccurenceReel += freq;
				}
				if (totalOccurenceReel == 0) { configIssues.Add ("The total occurence rate of symbol #" + (symbolIndex + 1).ToString() + " is 0."); }
			} else {
				int totalOccurence = slot.symbolFrequencies[symbolIndex];
				if (totalOccurence == 0) { configIssues.Add ("The total occurence rate of symbol #" + (symbolIndex + 1).ToString() + " is 0."); }
				//if (totalOccurence == 0) { configIssues.Add ("The total occurence rate of the slot is 0. It must be at least 1."); }
			}

			SpriteRenderer s1 = slot.symbolPrefabs[symbolIndex].GetComponent<SpriteRenderer>();
			MeshFilter s2 = slot.symbolPrefabs[symbolIndex].GetComponent<MeshFilter>();

			if (s1 != null)
			{
				if (!size.Equals(Vector3.zero))
				{
					if (s1.sprite != null)
					{
						if (size != s1.sprite.bounds.size) { configIssues.Add ("Symbol #" + (symbolIndex + 1) + " does not match the size of the other symbols."); continue; }
					} else {
						configIssues.Add ("Symbol #" + (symbolIndex + 1) + " has no sprite assigned.");
					}
				}
				if (s1.sprite != null)
					size = s1.sprite.bounds.size;
			}
			if (s2 != null)
			{
				if (!size.Equals(Vector3.zero))
				{
					if (size != s2.sharedMesh.bounds.size) { configIssues.Add ("Symbol #" + (symbolIndex + 1) + " does not match the size of the other symbols."); continue; }
				}
				size = s2.sharedMesh.bounds.size;

			}

		}
		for (int i = 0; i < slot.symbolSetNames.Count; i++)
		{
			if (slot.symbolSetNames[i] == "") configIssues.Add ("Specify a name for Symbol Set #" + (i + 1) + ".");
			//if (slot.symbolSets[i].symbols.Count == 0) configIssues.Add ("Specify at least one symbol for Symbol Set #" + (i+1).ToString());
		}
		int totalPossibleDraws = 0;
		for (int index = 0; index < slot.symbolSets.Count; index++)
		{
			if (slot.symbolSets[index].typeofSet == SetsType.scatter)
			{
				if (slot.setPays[index].pays.Count == 0)
				{
					configIssues.Add ("Symbol Set #" + (index + 1).ToString () + " is a scatter set, and it's symbols must be clamped in Symbol Setup.");
				}
				totalPossibleDraws += slot.setPays[index].pays.Count;
			} else {
				totalPossibleDraws += slot.numberOfReels * slot.reelHeight;
			}
		}
		if (totalPossibleDraws < slot.numberOfReels * slot.reelHeight)
		{
			configIssues.Add ("WARNING: Symbol draw requirement exceeds possible symbols, playing this slot will result in an infinite loop.");
		}

		bool canbet = false;
		foreach(BetsWrapper bet in slot.betsPerLine)
		{
			if (bet.canBet) canbet = true;
		}
		if (!canbet) configIssues.Add ("WARNING: you have no bets enabled, trying to increment the bet on this slot will result in an infinite loop.");

		//EditorUtility.DisplayCancelableProgressBar("test", "test", 0.9f);
		// PRINT OUT RESULT
	}
}
