using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum SlotState {
	ready,
	spinning,
	snapping,
	playingwins
}

public struct SlotComponents {
	public SlotCredits credits;
	public SlotWins wins;
	public SlotCompute compute;
	public SlotLines lines;
}

public class SlotWinData {
	public int lineNumber;
	public int paid;
	public int matches;
	public SetsType setType;
	public int setIndex;
	public string setName;
	public string readout;
	public List<GameObject>symbols;
	
	public SlotWinData(int line) {
		lineNumber = line;
		paid = 0;
		matches = 0;
		setType = SetsType.normal;
		setIndex = 0;
		setName = "";
		readout = "";
		symbols = new List<GameObject>();
	}
	public void setPaid(int pay) {
		paid = pay;
	}
}

public struct SlotScatterHitData {
	public int reel;
	public int hits;
	public SetsType setType;
	public int setIndex;
	public string setName;
	public GameObject symbol;
	
	public SlotScatterHitData(int reelIndex) {
		reel = reelIndex;
		hits = 0;
		setType = SetsType.normal;
		setIndex = 0;
		setName = "";
		symbol = null;
	} 
	
}

public class SlotGlobals {
	public const string PRODUCT_NAME = "Slots Creator Pro";
	public const string PRODUCT_VERSION = "1.03";
	public const string COMPANY = "Bulkhead Studios 2014";
	public const string MATH_PATH = "Assets/SlotMachinePlugin/Math/";
}
public class SlotHelp {
	// BASIC
	public const string NUMBER_OF_REELS = "Set the number of reels your slot will have.";
	public const string SYMBOL_HEIGHT = "Specify the total height of your reels in symbols.";
	public const string VERTICAL_INDENT = "Specify a top and bottom indent amount.  This indent is ignored by scatter calculations.";
	public const string BACKGROUND_PREFAB = "Specify a prefab to be used for the background of the reels.";
	public const string MAX_BET_PER_LINE = "Specify the maximum allowed bet per line.";
	public const string VALID_BETS = "These are the bet amounts that will cycle when the bet is changed. Specify each bet per line that will be allowed on this machine.  They should be in order of value from least to greatest.";
	public const string INITIAL_BET_PER_LINE = "The bet per line when the slot is first loaded.";
	public const string INITIAL_LINES_PLAYED = "The number of lines played when the slot is first loaded.";
	public const string PERSIST = "Enabling will save bet per line, lines played and credits between sessions.";
	public const string REEL_TIMING = "Set various timing elements of the slot machine.";
	public const string INITIAL_REEL_STOP_TIME = "This is the time from the moment you spin to the first reel coming to a stop.";
	public const string STOP_TIME_EACH_REEL = "This amount of time will elapse before each remaining reel will stop.";
	public const string WIN_DISPLAY_TIME = "This is the amount of time each win will be shown before moving on to the next.";
	public const string TIMEOUT_BETWEEN_WINS = "This is the time time to wait after showing a win before showing the next.";
	public const string SPIN_SPEED = "Arbitrary number to describe the speed in which the reels spin.  A higher number equates to a slower spin revolution.";
	public const string EASE_TIME = "Adjust length of ease effect applied to the stop of reels.";
	public const string EASE_TYPE = "Adjust the type of ease effect applied to the stop of the reels.";
	public const string USE_PREFAB_POOL = "This will toggle the use of a pregenerated pool of symbol prefabs for reel spinning.";
	public const string LINE_DRAWING = "Toggle the drawing of pay lines using Unity's Line Renderer.  Disable this is you plan on drawing your own lines, or simply not having lines.";
	public const string LINE_ZORDER = "Adjust the Z order of the Line Rendering.  For 2D, the default should do the trick.";
	public const string PAYLINE_WIDTH = "Increasing this will increase the width of the pay lines.";
	public const string PAYLINE_COLOR = "Specify colors for the paylines.";
	public const string PAYLINE_STROKE_WIDTH = "Determines the width of the pay line stroke. Set to 0 for no  stroke.";
	public const string PAYLINE_STROKE_COLOR = "Specify the color of the pay line outline.";
	// SYMBOL SETUP
	public const string SYMBOL_PRECISION = "Increase for larger symbol frequency ranges.";
	public const string SYMBOL_DECLARATIONS = "Assign GameObjects below to act as the symbols and winboxes for each symbol you add.  For the Symbol, required is the SlotSymbol script attached as well as SpriteRenderer or MeshFilter.";
	public const string PREFABS = "For eac.";
	public const string WINBOXES = "Assign GameObjects below to act as winboxes for each of your symbols.";
	public const string SYMBOL_PROPERTIES = "Here you will specify various symbol-specific properties.";
	public const string SYMBOL_OCCURENCE = "Specify the occurrence rate of your symbols here.  You can also Enable Frequency Per Reel to set occurence rates for each reel for this symbol.";
	public const string SYMBOL_CLAMP = "Limit the number of appearences of this symbol per reel and optionally total occurences on all reels.  Useful for Scatters and Bonus type symbols.";
	public const string CAP = "If you wish to limit the # of occurrences per reel for a symbol, do it below.  Scatter Set symbols require this be specified.";
	// SYMBOL SET SETUP
	public const string SET_NAME = "Name your set. ie Any Bar, Sevens";
	public const string SET_TYPE = "Specify set type here.";
	public const string SET_SYMBOLS = "Specify valid symbols for this set; add or remove them by using the buttons.";
	public const string SET_PAYS = "Specify the pays for this set; the first box would be 1 match, then 2 matches, etc.";
	public const string SET_ANTICIPATION = "Check to activate anticipation for subsequent reels following this number of matches.";
	// PAYLINE SET UP
	public const string PAYLINES = "Specify the position on each reel that the payline will evaluate.  Add and remove paylines as needed.";
	// COMPUTE
	public const string ACTIVE_RNG = "Change the Editor RNG in use.  NOTE: They vary in speed.";
	public const string CALLBACKS = "Drag your Monobehavior callback script here.";
	public const string ITERATIONS = "Total number of times to run simulation.  In paranthesis you will see estimated time to compute once it gears the speed of your computer.";
	public const string COMPUTE = "Run iterations on the current slot machine setup.  Use a low iteration count for quick feedback.";
	public const string AUTOCOMPUTE = "Automatically run the computations every time a setting changes.";
	public const string CSV = "Save output to comma delimited file.  Files can be found in SlotMachineFramework/Math/";
}	

public class SlotErrors {
	public const string MISSING_SYMBOL = "ERROR: Missing Game Object on Symbol. You must assign a Game Object to all symbols in Symbol Setup.";
	public const string NO_SYMBOLS = "ERROR: You must specify at least one reel symbol in Symbol Setup.";
	public const string MISSING_RENDERER = "ERROR: One of your symbol prefabs is missing a SpriteRenderer or MeshFilter.";
	public const string NO_LINES = "ERROR: You must define at least 1 payline in Payline Setup";
	public const string NO_SYMBOLSETS = "ERROR: To award pays, you must have at least one symbol set (create in Symbol Sets Setup).";
	public const string CLAMP_SCATTER = "ERROR: You must clamp scatter symbols.  See notification area for more info.";
}
