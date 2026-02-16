using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FinalBoss : MonoBehaviour
{
    Animator anim;
    public GameObject dialogueBox;
    public GameObject effects;
    public TMP_Text text;
    public bool IsTalking { get; private set; }

    public float timeBetweenLetters;
    public float timeBetweenStatements;
    public float riseSpeed;
    public float riseLength;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public IEnumerator Talk()
    {
        IsTalking = true;
        anim.SetBool("Talking", true);
        dialogueBox.SetActive(true);

        string[] monologue = new string[] {
                            "Greetings, rock. " ,
                            "I've been following your accomplishments. " ,
                            "You managed to kill my entire lunar excavation team. " ,
                            "But you're too late!" ,
                            "Behold! "
                            };
        string[] monologuePart2 = new string[] {
                            "I have captured your mother!" ,
                            "And your son!" ,
                            "And now I have aquired the powers of a GOD!" ,
                            "Grovel, little rock! " ,
                            "Even the power of the EVIL Inc. Ultimate Grappling Hook of Greatness 59.99$ please buy please buy buy buy we need money please™" ,
                            "is nothing compared to my power!"
                            };

        foreach (string line in monologue)
        {
            text.text = ""+line[0];
            yield return new WaitForSeconds(timeBetweenLetters);
            for (int i = 1; i < line.Length; i++)
            {
                text.text += line[i];
                yield return new WaitForSeconds(timeBetweenLetters);
            }

            yield return new WaitForSeconds(timeBetweenStatements);
        }

        Instantiate(effects, gameObject.transform);
        StartCoroutine(Rise());

        foreach (string line in monologuePart2)
        {
            text.text = "" + line[0];
            yield return new WaitForSeconds(timeBetweenLetters);
            for (int i = 1; i < line.Length; i++)
            {
                text.text += line[i];
                yield return new WaitForSeconds(timeBetweenLetters);
            }

            yield return new WaitForSeconds(timeBetweenStatements);
        }

        StopCoroutine(Rise());
        anim.SetBool("Talking", false);
        dialogueBox.SetActive(false);
        IsTalking = false;
    }

    IEnumerator Rise()
    {
        for (int i = 0; i < riseLength; i++)
        {
            transform.position += Time.deltaTime * riseSpeed * Vector3.up;
            yield return new WaitForFixedUpdate();
        }
    }
}
