
using UnityEngine;

public class CharAnim : MonoBehaviour
{
    private Animator charAnim;
    private GameObject selfChar;

    private void Start()
    {
        charAnim = GetComponent<Animator>();
    }

    private void Update()
    {
        characterMotion();
    }

    void characterMotion()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            charAnim.SetTrigger("ConfusedTrigger");
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            charAnim.SetTrigger("SwingTrigger");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            charAnim.SetTrigger("ShyTrigger");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            charAnim.SetTrigger("Happy01Trigger");
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            charAnim.SetTrigger("Happy02Trigger");
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            charAnim.SetTrigger("Happy03Trigger");
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            charAnim.SetTrigger("Happy04Trigger");
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            charAnim.SetTrigger("MadTrigger");
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            charAnim.SetTrigger("SadTrigger");
        }
    }
}
