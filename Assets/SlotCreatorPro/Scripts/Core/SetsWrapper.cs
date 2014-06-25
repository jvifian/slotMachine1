using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum SetsType {
	normal,
	scatter
}
[System.Serializable]
public class SetsWrapper
{
	public SetsType typeofSet;
	public List<int> symbols;
	public int scatterCount;
	public bool allowWilds = true;
}