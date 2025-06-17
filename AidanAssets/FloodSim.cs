/*
Script by Aidan Weigel :3
6/12/25
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;

public class FloodSim : MonoBehaviour
{
    #region Object Variables
    public GameObject floodClear;
        private float floodClear_StartOpacity;
        [SerializeField] private MeshRenderer floodClearMR; 
    public GameObject floodGrey;
        private float floodGrey_StartOpacity;
        [SerializeField] private MeshRenderer floodGreyMR; 
    public GameObject floodBlack;
        private float floodBlack_StartOpacity;
        [SerializeField] private MeshRenderer floodBlackMR;
        
    public GameObject anchor; //should be the 'flooding' object parent
    #endregion

    #region Movement and Time

    /*
        current start = -0.289

        current end = -0.146 
    */
    [SerializeField] private float desiredStart = -0.3f; //water start height (out of frame)
    [SerializeField] private float desiredEndHigh = -0.145f; //water end height HIGH
   // [SerializeField] private float desiredEndLow = -0.15f; //water end height LOW
    [SerializeField] private float fadeDuration = 3f; //duration of opacity transitions
    private Coroutine movingObjects = null;
    private Coroutine fadingClear = null;
        private Coroutine fadingGrey = null;
        private Coroutine fadingBlack = null;
    private int floodIsOn = 0;
    public bool DEBUG_MODE = true; //whether the flood test runs or not
    private bool buttonsOnCooldown = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        if (floodClear == null)
        {
            Debug.LogError("clear flood object(s) not assigned");
        }
        else
        { //if those game objects are ok, do stuff
            floodClear_StartOpacity = floodClearMR.material.color.a;
            SetOpacity(floodClearMR, 0f);
        }
        if (floodGrey == null)
        {
            Debug.LogError("grey flood object(s) not assigned");
        }
        else
        { //if those game objects are ok, do stuff
            floodGrey_StartOpacity = floodGreyMR.material.color.a;
            SetOpacity(floodGreyMR, 0f);
        }
        if (floodBlack == null)
        {
            Debug.LogError("black flood object(s) not assigned");
        }
        else
        { //if those game objects are ok, do stuff
            floodBlack_StartOpacity = floodBlackMR.material.color.a;
            SetOpacity(floodBlackMR, 0f);
        }

    }//end of start method

    //TEST METHOD
    float timer = 0f;
    bool upOrDown = true; //true is up
    int cycleThrough = 0;
    bool debugWasOn = false;

    void Update() {
        if (DEBUG_MODE)
        { //hopefully this isn't too heavy
            if (!debugWasOn) debugWasOn = true;
            timer += Time.deltaTime;

            if (timer >= 3f)
            {
                if (movingObjects != null) StopCoroutine(movingObjects); //stop any other movement
               // if (upOrDown) { waterRiseHigh(3); upOrDown = false; }
              //  else { waterReturn(3); upOrDown = true; }
               //  waterButton(3);
                cycleThrough++;
                if (cycleThrough % 2 == 0 && cycleThrough != 4)
                {
                    waterButton(1);
                }
                else if (cycleThrough % 3 == 0)
                {
                    waterButton(2);
                }
                else if (cycleThrough % 5 == 0)
                {
                    waterButton(3);
                }
                else if (cycleThrough % 7 == 0)
                {
                    waterButton(4);
                    cycleThrough = 0;
                }

                timer = 0f;
            }
        }
        else if (debugWasOn)
        {
            waterReturn(3);
            if (fadingBlack == null) fadingBlack = StartCoroutine(SetOpacitySlow(floodBlackMR, 0f, 5f));
            debugWasOn = false;
        }
    }

    //END OF TEST METHOD


    #region waterMovement

    /*
        1 = clear, 2 = grey, 3 = black
    */
    private IEnumerator buttonDisableRn() //disable all buttons until the small cooldown is done
    {
        buttonsOnCooldown = true;
        yield return new WaitForSeconds(0.3f);
        buttonsOnCooldown = false;
    }

    public void waterButton(int whichFlood)
    {
        if (buttonsOnCooldown) return; //to prevent double-clicking and rapid clicking
        else
        {
            StartCoroutine(buttonDisableRn());
        }

        if (floodIsOn == 0 && whichFlood != 4)
        { //nothing is currently flooding

            waterRiseHigh(whichFlood);
            floodIsOn = whichFlood;
        }
        else if (whichFlood == 4)
        { //if reset button is pressed
            for (int i = 1; i <= 3; i++)
            {
                waterReturn(i); //return everything
            }//endof for
            floodIsOn = 0;
        }
        else
        { //if one thing is flooding
            waterReturn(floodIsOn);
            if (whichFlood == floodIsOn) floodIsOn = 0; //if they pressed the same button to recall the flood
            else
            {
                /* If the user pressed a button that wasn't resetting a previous flood,
                    then return the previous flood and recursive call for the new flood.
                */
                floodIsOn = 0;
                waterButton(whichFlood);
            }

        }

    }//endof method

    public void waterRiseHigh(int whichFlood) { //will cause the 'water' to rise high
        //each case should fade in the object, then slowly rise it from y:-0.3 to y:-0.145. These numbers may change
        if (movingObjects != null) StopCoroutine(movingObjects); //stop any other movement

        //can put this SE up here because theres no non-activation chance for this method
        SoundManagerScript.Instance.PlayAudioClip("biggerWave", false);

        switch (whichFlood)
        {
            case 1: //clear
                    //set opacity to the start opacity, then make it go up with an IEnumerator method
                if (!floodClear.activeSelf) floodClear.SetActive(true);
                if (fadingClear != null) StopCoroutine(fadingClear);
                fadingClear = StartCoroutine(SetOpacitySlow(floodClearMR, floodClear_StartOpacity, fadeDuration));
                movingObjects = StartCoroutine(verticalMove(floodClear, (float)anchor.transform.position.y + desiredEndHigh));
                break;
            case 2: //grey
                if (!floodGrey.activeSelf) floodGrey.SetActive(true);
                if (fadingGrey != null) StopCoroutine(fadingGrey);
                fadingGrey = StartCoroutine(SetOpacitySlow(floodGreyMR, floodGrey_StartOpacity, fadeDuration));
                movingObjects = StartCoroutine(verticalMove(floodGrey, (float)anchor.transform.position.y + desiredEndHigh));
                break;
            case 3: //black
                if (!floodBlack.activeSelf) floodBlack.SetActive(true);
                if (fadingBlack != null) StopCoroutine(fadingBlack);
                fadingBlack = StartCoroutine(SetOpacitySlow(floodBlackMR, floodBlack_StartOpacity, fadeDuration));
                movingObjects = StartCoroutine(verticalMove(floodBlack, (float)anchor.transform.position.y + desiredEndHigh));
                break;
            default: //nothing happens
                break;
        }
    }//endof method

    public void waterReturn(int whichFlood) {
        if (movingObjects != null) StopCoroutine(movingObjects); //stop any other movement

        switch (whichFlood)
        {
            case 1: //clear
                //set opacity to the start opacity, then make it go up with an IEnumerator method
                if (!floodClear.activeSelf) return; //because it makes no sense to 'turn off' a thing thats not on
                SoundManagerScript.Instance.PlayAudioClip("waveRush", false); //sound effect
                if (fadingClear != null) StopCoroutine(fadingClear);
                fadingClear = StartCoroutine(SetOpacitySlow(floodClearMR, 0f, fadeDuration));
                StartCoroutine(verticalMove(floodClear, (float)anchor.transform.position.y + desiredStart)); //not using a coroutine variable so they always return no matter what
                // movingObjects = StartCoroutine(verticalMove(floodGrey, desiredStart));
                break;
            case 2: //grey
                if (!floodGrey.activeSelf) return;
                SoundManagerScript.Instance.PlayAudioClip("waveRush", false); 
                if (fadingGrey != null) StopCoroutine(fadingGrey);
                fadingGrey = StartCoroutine(SetOpacitySlow(floodGreyMR, 0f, fadeDuration));
                StartCoroutine(verticalMove(floodGrey, (float)anchor.transform.position.y + desiredStart));
                // movingObjects = StartCoroutine(verticalMove(floodGrey, desiredStart));
                break;
            case 3: //black
                if (!floodBlack.activeSelf) return;
                SoundManagerScript.Instance.PlayAudioClip("waveRush", false); 
                if (fadingBlack != null) StopCoroutine(fadingBlack);
                fadingBlack = StartCoroutine(SetOpacitySlow(floodBlackMR, 0f, fadeDuration));
                StartCoroutine(verticalMove(floodBlack, (float)anchor.transform.position.y + desiredStart));
                // movingObjects = StartCoroutine(verticalMove(floodGrey, desiredStart));
                break;
             default: //nothing happens
                break;
        }
    }

    private IEnumerator verticalMove(GameObject targetObject, float desiredHeight) {
        float elapsed = 0f;
        float duration = Mathf.Abs(desiredHeight - targetObject.transform.position.y) / 0.1f; // distance divided by speed

        Vector3 startPos = targetObject.transform.position;
        Vector3 endPos = new Vector3(startPos.x, desiredHeight, startPos.z);

        while (elapsed < duration)
        {
            targetObject.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }//endof while

        targetObject.transform.position = endPos;
    }//endof method
    
    #endregion

    #region OpacityMethods

    private void SetOpacity(MeshRenderer targetMesh, float targetOpacity)
    { //set opacity immediately
        Material targetMat = targetMesh.material;

        if (targetOpacity == 0f)
        {
            targetMesh.gameObject.SetActive(false); //turn off object if its mesh is invisible
        }
        else
        {
            targetMesh.gameObject.SetActive(true); //turn on object if mesh is visible
        }
        targetMat.color = new Color(targetMat.color.r,
                                            targetMat.color.g,
                                            targetMat.color.b,
                                            targetOpacity); //immediately set opacity
    }//endof method

    private IEnumerator SetOpacitySlow(MeshRenderer targetMesh, float targetOpacity, float duration) { //set opacity slower
                                            
        if (!targetMesh.gameObject.activeSelf) targetMesh.gameObject.SetActive(true); //in case the object isnt active at first
        Material targetMat = targetMesh.material; //target material (different from spriterenderer)
        
        float elapsed = 0f, t = 0f, newOpacity = 0f; //elapsed time, mathClamp alpha, new alpha during while loop
        float startOpacity = targetMat.color.a;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            t = Mathf.Clamp01(elapsed / duration);

            newOpacity = Mathf.Lerp(startOpacity, targetOpacity, t);
            targetMat.color = new Color (targetMat.color.r, 
                                            targetMat.color.g, 
                                            targetMat.color.b,  
                                            newOpacity); //just changes the opacity

            yield return null; //wait for operations
        }//endof while 

        targetMat.color = new Color (targetMat.color.r, 
                                            targetMat.color.g, 
                                            targetMat.color.b, 
                                            targetOpacity); //make sure its the correct opacity at the end

        if (targetOpacity == 0f) {
            //do things after its invisible, e.g. turn off the gameobject
            targetMesh.gameObject.SetActive(false);
        }


    } //endof method

    #endregion

}//endof class
