// Brad Lima - Bulkhead Studios 2014

using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
	public bool loadCredits;
	public int credits;
	public GUISkin skin;

	private GameObject title;

	[HideInInspector]
	public Slot slot;

	private static SlotManager s_Instance = null;
	public static SlotManager instance { 
		get {
			if (s_Instance == null) {
				s_Instance =  FindObjectOfType(typeof (SlotManager)) as SlotManager;
			}
			
			if (s_Instance == null) {
				GameObject obj = new GameObject("SlotManager");
				s_Instance = obj.AddComponent(typeof (SlotManager)) as SlotManager;
			}

			return s_Instance;
		}
	}
	
	void OnApplicationQuit() {
		s_Instance = null;
	}

	void Awake() {
		if (loadCredits)
		{
			credits = PlayerPrefs.GetInt("credits", credits);
		}

		title = GameObject.Find ("title");
	}

	void saveCredits() {
		PlayerPrefs.SetInt("credits", credits); 
	}

	#region Load / Unload
	public void loadSlot(string slotPrefabName)
	{
		GameObject go = (GameObject)Instantiate (Resources.Load (slotPrefabName));
		slot = go.GetComponent<Slot>();
		slot.refs.credits.setCredits(credits);
	}
	public void unloadSlot()
	{
		credits = slot.refs.credits.withdrawCredits();
		saveCredits();
		Destroy (slot.gameObject);

	}
	#endregion

	void OnGUI() {
		GUI.skin = skin;

		if (slot == null)
		{
			GUILayout.BeginArea(new Rect (Screen.width/4, Screen.height/3, Screen.width/2, Screen.height/2));
			GUILayout.BeginHorizontal("box");
			GUIStyle style = new GUIStyle();
			style.fontSize = 20;
			if (GUILayout.Button("3-Reel 2D Slot"))
			{
				title.SetActive(false);
				loadSlot("3ReelSlot2D");
			}
/*			if (GUILayout.Button("5-Reel 2D Slot"))
			{
				title.SetActive(false);
				loadSlot("5ReelSlot2D");
			}*/
			if (GUILayout.Button("3-Reel Retro 2D Slot"))
			{
				title.SetActive(false);
				loadSlot("3ReelRetroSlot2D");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect (Screen.width/4, Screen.height/3+50, Screen.width/2, Screen.height/2));
			GUILayout.BeginHorizontal("box");
			if (GUILayout.Button("5-Reel Retro 2D Slot"))
			{
				title.SetActive(false);
				loadSlot("5ReelRetroSlot2D");
			}
			if (GUILayout.Button( "Hearts Attack"))
			{
				title.SetActive(false);
				loadSlot("TinyRetro");
			}
			if (GUILayout.Button( "3D Example"))
			{
				title.SetActive(false);
				loadSlot("3DSlot");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

		} else {
			if (GUI.Button(new Rect(50, 25, 100, 50), "Exit Slot"))
			{
				unloadSlot();
				title.SetActive(true);
			}
		}
	}
}