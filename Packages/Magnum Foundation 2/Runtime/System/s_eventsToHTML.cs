using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagnumFoundation2.Objects;
using MagnumFoundation2.System;
using System.IO;

public class s_eventsToHTML : MonoBehaviour
{
    [System.Serializable]
    public struct scrComp {
        public string context;
        public ev_script script;
    }
    public scrComp[] scripts;
    string html = "";
    public string scriptName;
    protected ev_details prevScript;

    public void WriteLine(string stuff) {
        html += stuff;
    }

    public void ScriptToHTML() {

        html = "";
        WriteLine("<!DOCTYPE html>");
        WriteLine("<html>");
        WriteLine("<style>.cnt {text-align: center;  }</style>");
        WriteLine("<h1><div class ='cnt' >" + scriptName + "</div></h1>");
        foreach (scrComp cmp in scripts)
        {
            WriteLine("<h2 style ='text-align: center;'><i>Context - " + cmp.context + "</i></h2>");
            foreach (ev_details a in cmp.script.elements)
            {
                if (a != null)
                    ConvertScript(a);
            }
            prevScript = null;
        }
        WriteLine("</body>");
        WriteLine("</html>");
        File.WriteAllText("script.html", html);
        print("Here's your script");
    }

    protected virtual void ConvertScript(ev_details script) {
        switch (script.eventType) {
            case EVENT_TYPES.DIALOGUE:
                if (prevScript != null)
                {
                    if (prevScript.eventType == EVENT_TYPES.DIALOGUE)
                    {
                        if (prevScript.string1 != script.string1)
                        {
                            if (script.string1 == "")
                            {
                                WriteLine("<h3>Narrator</h3>");
                            }
                            else
                            {
                                s_triggerhandler trig = s_triggerhandler.GetInstance();
                                name_colour colour = trig.nameColours.Find(x => x.name == script.string1);
                                if (colour != null)
                                {
                                    WriteLine("<h3 style='color:#" + ColorUtility.ToHtmlStringRGB(colour.colour) + ";'>" + script.string1 + "</h3>");
                                }
                                else
                                {
                                    WriteLine("<h3>" + script.string1 + "</h3>");
                                }
                            }
                            prevScript = script;
                        }
                    }
                }
                else
                {
                    if (script.string1 == "")
                    {
                        WriteLine("<h3>Narrator</h3>");
                    }
                    else
                    {
                        WriteLine("<h3>" + script.string1 + "</h3>");
                    }
                    prevScript = script;
                }
                WriteLine("<p>" + script.string0 + "</p>");
                break;

            case EVENT_TYPES.WAIT:
                return;

            default:
                prevScript = null;
                break;
        }
    }

}
