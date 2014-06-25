using UnityEngine;
using System.Collections;


[System.Serializable]
public class SaveEditor
{
	public bool autoCompute;
	public float returnPercent;
	public int returnTotalWon;
	public int returnTotalBet;
	public string returnData;
	public int returnPercentItterations = 10000;

	public bool showBasicSettingsPanel;
	public bool showSymbolSetupPanel;
	public bool symbolDeclarations;
	public bool showOccurences;
	public bool showSymbolSetsPanel;
	public bool showLinesSetupPanel;
	public bool showReturnPanel;
	public bool showNotificationsPanel = true;
}
