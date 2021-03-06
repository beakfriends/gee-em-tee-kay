using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugStuff : MonoBehaviour
{
    public bool skipTitleScreen = false;
    public bool skipIntroDialogue = false;
    public bool skipDailyDialogue = false;
    public float debugCycleSpeed = 1;
    public bool cycleStemColour = false;
    public bool cyclePossibleFlowerColours = false;
    public bool cycleFlowerColourSat = false;
    public bool cycleFlowerColourValue = false;
    public int dayToPlayDialogueFor_1Indexed = -1;
    public bool keyQGrowsPlant = false;
}
