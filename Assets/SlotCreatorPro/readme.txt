Thanks for purchasing Slots Creator Pro!

Webplayer example:
http://www.bulkheadstudios.com/assets/scp
Docs:
https://docs.google.com/document/d/1gUCZFV8hVNaUsmWiJZASEWzPSBs5Hae8bYPVUw0gNKk/edit
Unity Forum Thread:
http://forum.unity3d.com/threads/237410-Slots-Creator-Pro-%28Slot-Machine-maker%29?p=1576516#post1576516

Changelog:

V1.01

-Few minor bug fixes

V1.02

-Changed the way you set up bets per line. Rather than just a linear max bet per line, you now specify each bet per line, and you can also control which are enabled and disable.
-Added Callback	OnIncreasedBetPerLine (int bet)
-Added Callback OnIncreasedLinesPlayed (int linesPlayed)
-Added Callback OnBeginDelayBetweenLineWinDisplayed (SlotWinData data)
-Added Callback OnScatterSymbolLanded - fired when scatter symbol is landing on a valid reel position; half way through the reel ease out tween.
-Callback OnBeginSpin now returns the last displayed Win, or null if there wasn't one.
-Added bet per line and lines played to Math compiler as a result of the bet changes made.  Now you can compute a return based on any number of lines and bets per line.
-Fixed a bug with calculating the math on scatter symbols that had a maximum total occurences set.
-Added winbox pooling which will automatically be enabled with Use Pool is true.
-Ability to reorder symbol sets pays (beta)
-Added callback OnReelStop(int)
-Added callback OnBeginCreditWinCountOff
-Added callback OnBeginCreditBonusCountOff
-Added callback OnCompletedCreditCountOff
-Fixed bugs

Note: Although this version is backwards compatible, because of the changes to the way you define betting, you will need to specify some new settings.  Default is just one bet of 1.

V1.03

-Added OnCompletedBonusCreditCountOff callback
-Added scatterCount parameter to OnScatterSymbolLanded which will now also return the total scatter count on the symbol's set that hit
-Added spin(int[,]) overload that takes an array of int that represents the symbol indexes for explicitly specifying the result of the spin.  This makes it easy to feed SCP results from another source.
-Added pauseWins and resumeWins to SlotWins class, which pauses and resumes the playing of wins.
-Fixed bug in up/down reorder symbol sets.
-Added totalIn and totalOut to SlotCredits
-Added awardBonus to SlotCredits which you can specify whether that amount is added to the totalIn or not - useful for when your game has other ways besides traditional ways to add credits, such as bonus games.
-Added isFirstLoop boolean to the OnLineWinDisplayed callback, useful if you want to do something only the first time a particular win is displayed (such as play a sound, or animation)
-Minor bug fixes

Note:  The OnLineWinDisplayed callback will need to be altered upon upgrading.  See above notes.