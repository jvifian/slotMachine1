using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotLines : MonoBehaviour {

	public bool linesEnabled = true;
	public float linesZorder = -0.001f;
	public Shader linesShader = null;
	public float payLineWidth = 0.1f;
	public float strokeWidth = 0.075f;
	public Color payLineColor1 = Color.white;
	public Color payLineColor2 = Color.white;
	public Color strokeColor = Color.black;

	private Slot slot;

	private List<GameObject> lineRenderers = new List<GameObject>();
	private GameObject lineContainer;

	// Use this for initialization
	void Start () {
		slot = GetComponent<Slot>();

		if (linesEnabled)
			createPaylines();

	}

	void OnEnable()
	{

	}
	#region Show Line
	public void displayLines(int upTo)
	{
		if (!linesEnabled) return;
		for (int i = 0; i < lineRenderers.Count; i++)
		{
			if (i < upTo)
			{
				lineRenderers[i].SetActive(true);
			} else {
				lineRenderers[i].SetActive(false);
			}
		}
	}
	public void hideLines()
	{
		if (!linesEnabled) return;
		for (int i = 0; i < lineRenderers.Count; i++)
		{
			lineRenderers[i].SetActive(false);
		}
	}
	
	#endregion
	
	#region Create Lines
	void createLineRenderer(int lineNumber, List<Vector3> points, float width, Color c1, Color c2)
	{
		List<int> pos = GetComponent<Slot>().lines[lineNumber].positions;
		
		GameObject go = new GameObject();
		go.transform.parent = lineRenderers[lineNumber].transform;
		go.name = "Line_" + lineNumber;
		
		LineRenderer lineRenderer = go.AddComponent<LineRenderer>();

		if (linesShader != null)
		{
			lineRenderer.material = new Material(linesShader);
		}
		lineRenderer.SetColors(c1, c2);
		lineRenderer.SetWidth(width, width);
		lineRenderer.SetVertexCount(pos.Count + 2);

		for (int i = 0; i < points.Count; i++)
		{
			lineRenderer.SetPosition(i, points[i]);
		}
		//if (reverse) points.Reverse();
		
		//lineRenderer.enabled = false;
		
	}
	List<Vector3> createPoints(int lineNumber)
	{
		
		List<int> pos = GetComponent<Slot>().lines[lineNumber].positions;
		List<Vector3> points = new List<Vector3>();
		
		Vector3 sp = Vector3.zero;
		float z = linesZorder + ((-0.0001f) * lineRenderers.Count);
		for (int ii = 0; ii < pos.Count; ii++)
		{
			GameObject symbol = GetComponent<Slot>().reels[ii].symbols[pos[ii]];
			sp = symbol.transform.position;
			if (ii == 0)
			{
				Vector3 startVec = new Vector3(GetComponent<Slot>().reels[0].transform.position.x - slot.reels[ii].GetComponent<SlotReel>().symbolWidth, sp.y, z);
				points.Add (startVec);
			}
			Vector3 vec = new Vector3(GetComponent<Slot>().reels[ii].transform.position.x, sp.y, z);
			points.Add (vec);
		}
		Vector3 endVec = new Vector3(GetComponent<Slot>().reels[pos.Count-1].transform.position.x + slot.reels[pos.Count-1].GetComponent<SlotReel>().symbolWidth, sp.y, z);
		points.Add (endVec);
		
		return points;
	}
	
	public void createPaylines()
	{
		lineContainer = new GameObject();
		lineContainer.name = "Lines";
		lineContainer.transform.parent = transform;
		
		List<Vector3> points;
		
		for (int lineNumber = 0; lineNumber < GetComponent<Slot>().lines.Count; lineNumber++)
		{
			points = createPoints (lineNumber);
			
			GameObject lineC = new GameObject("Line" + lineNumber);
			lineC.transform.parent = lineContainer.transform;
			lineRenderers.Add (lineC);
			lineC.SetActive(false);
			
			createLineRenderer(lineNumber, points, payLineWidth, payLineColor1, payLineColor2);
			if (strokeWidth > 0)
				createLineRenderer(lineNumber, points, payLineWidth + strokeWidth, strokeColor, strokeColor);
			points.Reverse();
			createLineRenderer(lineNumber, points, payLineWidth, payLineColor1, payLineColor2);
			if (strokeWidth > 0)
				createLineRenderer(lineNumber, points, payLineWidth + strokeWidth, strokeColor, strokeColor);
		}
	}
	#endregion
}
