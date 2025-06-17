using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonCooldown : MonoBehaviour
{
    [SerializeField] private Button myButton;
    public float cooldownTime = 0.3f;

    public void ButtonCooldownTime()
    {
        //literally just disable it when pressed, for like 0.3 seconds
        StartCoroutine(DisableTemporarily());
    }

    private IEnumerator DisableTemporarily()
    {
        myButton.interactable = false;
        yield return new WaitForSeconds(cooldownTime);
        myButton.interactable = true;
    }
}
