using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuiTest : MonoBehaviour
{
    public GUISkin mySkin;
    private bool toggle = true;

    void OnGUI()
    {
        // Assign the skin to be the one currently used.
        GUI.skin = mySkin;

        // Make a toggle. This will get the "button" style from the skin assigned to mySkin.
        toggle = GUI.Toggle(new Rect(10, 10, 150, 20), toggle, "Skinned Button", "button");

        // Assign the currently skin to be Unity's default.
        GUI.skin = null;

        // Make a button. This will get the default "button" style from the built-in skin.
        GUI.Button(new Rect(10, 35, 150, 20), "Built-in Button");
    }
}
