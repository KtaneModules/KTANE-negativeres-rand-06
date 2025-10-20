using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class negativeResScript : MonoBehaviour
{
    public KMBombModule Module;
    //public KMSelectable Cover;
    //public KMSelectable Status;
    //public KMSelectable[] spheres;
    public GameObject[] spheres;
    public TextMesh Center;
    public KMAudio Audio;
    public AudioClip[] sounds;
    //public AudioClip[] clips = new AudioClip[3];

    
    static int ModuleIdCounter;
    int ModuleId;
    private bool secondStage;
    static private int dimensions = 6;
    private int[] config;
    private int[] shifts;
    private int[] currentPosition;
    private Color[] colors = new Color[1 << dimensions];
    private Vector3[] positionVectors = new Vector3[1 << dimensions];
    private Vector3[] shiftedPositionVectors = new Vector3[1 << dimensions];
    private string axisNames = "XYZWVURSTOPQLMNIJKFGHCDEAB1"; //not that i'm making NRes for higher dimensions but why not
    private List<int> currentAnswer;
    private string readableConfig;
    private bool lockInput = false;

    void initialize()
    {
        secondStage = false;
        currentAnswer = new List<int>();
        readableConfig = "";
        config = Enumerable.Range(1, dimensions).ToArray();
        generateConfig();
        Center.text = config.Length.ToString();
        shifts = positionsFromConfig();
        currentPosition = Enumerable.Range(0, shifts.Length).ToArray();
        for (int i = 0; i < 1 << dimensions; i++)
        {
            shiftedPositionVectors[i] = xyzFromNumber(shifts[i]);
            spheres[i].GetComponent<MeshRenderer>().material.color = colors[i];
        }
    }

    IEnumerator strike()
    {
        Audio.PlaySoundAtTransform(sounds[1].name, transform);
        lockInput = true;
        Debug.LogFormat("[Negative-Reesistance #{0}] You've entered {1}, which is wrong. Regenerating.", ModuleId, Center.text);
        float time2 = 0f;
        Color[] buttonCurrentColors = new Color[1 << dimensions];
        for  (int i = 0; i < 1 << dimensions; i++)
            buttonCurrentColors[i]=spheres[i].GetComponent<MeshRenderer>().material.color;
        while (time2 < 1f)
        {
            for (int i = 0; i < 1 << dimensions; i++)
            {
                spheres[i].GetComponent<MeshRenderer>().material.color = Color.Lerp(buttonCurrentColors[i], colors[i], time2);
            }
            yield return new WaitForSeconds(.1f);
            time2 += .1f;
        }
        yield return new  WaitForSeconds(2f);
        Module.HandleStrike();
        Center.text = config.Length.ToString();
        lockInput = false;
        initialize();
        StartCoroutine(move());
    }
    int[] positionsFromConfig()
    {
        int[] ans = new int[1 << config.Length];
        for (int i = 0; i < 1 << config.Length; i++)
        {
            for (int j = 0; j < config.Length; j++)
            {
                if ((config[j] < 0) ^ (((1 << j) & i)!=0)) ans[i] += 1<<(config[j]*(config[j]<0?-1:1)-1);
            }
        }
        return ans;
    }
    Vector3 xyzFromNumber(int num)
    {
        int x = 0, y = 0, z = 0;
        if ((num & 1) != 0) x += 10;
        if ((num & 2) != 0) y += 10;
        if ((num & 4) != 0) z += 10;
        if ((num & 8) != 0) { x += 8; y += 2; z += 5; }
        if ((num & 16) != 0) { x += 2; y += 5; z += 8; }
        if ((num & 32) != 0) { x += 5; y += 8; z += 2; }
        return new Vector3(x/10f,y/10f,z/10f);
    }
    IEnumerator move()
    {
        while (!secondStage)
        {
            float time = 0f;
            while (time < 1.03f)
            {
                for (int i = 0; i < 64; i++)
                    spheres[i].transform.localPosition = Vector3.Lerp(
                        positionVectors[currentPosition[i]],
                        shiftedPositionVectors[currentPosition[i]], time);
                yield return new WaitForSeconds(.05f);
                time += .05f;
            }
            int[] tmp = new int[64];
            Array.Copy(currentPosition, tmp, tmp.Length);
            for (int i = 0; i < 64; i++) currentPosition[i] = shifts[tmp[i]];
            yield return new WaitForSeconds(3f);
        }

        
        float time2 = 0f;
        while (time2 < 1f)
        {
            for (int i = 0; i < 64; i++)
            {
                spheres[i].GetComponent<MeshRenderer>().material.color = Color.Lerp(colors[i], Color.gray, time2);
            }
            yield return new WaitForSeconds(.1f);
            time2 += .1f;
        }
        for (int i = 0; i < 64; i++)
        {
            spheres[i].transform.localPosition = xyzFromNumber(i);
        }
        lockInput = false;
        Center.text = "Ready.";
        yield return null;
    }

    void generateConfig()
    {
        int[] axis = Enumerable.Range(1,dimensions).ToList().Shuffle().GetRange(0,dimensions-1).ToArray();
        bool[] signs = new bool[dimensions-1];
        for (int i = 0; i < dimensions-1; i++) signs[i] = Random.Range(0, 2) == 1;
        for (int i = 0; i < dimensions-1; i++) readableConfig += (signs[i]?"+":"-") + axisNames[axis[i]-1];
        Debug.LogFormat("[Negative-Resistance #{0}] Your answer is: {1}.",ModuleId, readableConfig);
        for (int i = 0; i < dimensions-1; i++) 
            config[axis[i]-1] = axis[(i + 1) % (dimensions-1)] * (signs[(i + 1) % (dimensions-1)] ? 1 : -1);
    }

    void updateText()
    {
        if (currentAnswer.Count == 0)
        {
            Center.text = "Ready.";
            for (int i = 0; i < 1 << dimensions; i++) spheres[i].GetComponent<MeshRenderer>().material.color = Color.gray;
            return;
        }
        for (int i = 0; i < 1 << dimensions; i++)
        {
            if (i == currentAnswer.First()) spheres[i].GetComponent<MeshRenderer>().material.color = Color.green;
            else if (i == currentAnswer.Last()) spheres[i].GetComponent<MeshRenderer>().material.color = Color.red;
            else if (currentAnswer.Contains(i)) spheres[i].GetComponent<MeshRenderer>().material.color = Color.white;
            else if ((((i ^ currentAnswer.Last()) - 1) & (i ^ currentAnswer.Last())) == 0)
                spheres[i].GetComponent<MeshRenderer>().material.color = Color.gray;
            else spheres[i].GetComponent<MeshRenderer>().material.color = Color.black;
        }
        string ans = "";
        for (int i = 0; i < currentAnswer.Count-1; i++)
        {
            switch (currentAnswer[i + 1] - currentAnswer[i])
            {
                case 1:   ans += "+X"; break;
                case -1:  ans += "-X"; break;
                case 2:   ans += "+Y"; break;
                case -2:  ans += "-Y"; break;
                case 4:   ans += "+Z"; break;
                case -4:  ans += "-Z"; break;
                case 8:   ans += "+W"; break;
                case -8:  ans += "-W"; break;
                case 16:  ans += "+V"; break;
                case -16: ans += "-V"; break;
                case 32:  ans += "+U"; break;
                case -32: ans += "-U"; break;
                default: Center.text="?"; return;
            }
        }
        Center.text = ans;
    }

    void checkAnswer()
    {
        string[] configs = new string[readableConfig.Length / 2];
        for (int i = 0; i < readableConfig.Length; i+=2) configs[i/2] = readableConfig.Substring(i) + readableConfig.Substring(0, i);
        if (configs.Contains(Center.text)) StartCoroutine(solve());
        else StartCoroutine(strike());
    }

    IEnumerator solve()
    {
        lockInput = true;
        Audio.PlaySoundAtTransform(sounds[0].name, transform);
        Center.text = "";
        yield return new WaitForSeconds(3f);
        List<int> tmp = Enumerable.Range(0, 1<<dimensions).ToList().Shuffle();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 1 << (dimensions - 3); j++)
            {
                spheres[tmp[i*(1 << (dimensions - 3))+j]].SetActive(false);
            }
            Audio.PlaySoundAtTransform(sounds[1].name, transform);
            yield return new WaitForSeconds(.5f);
        }
        Audio.PlaySoundAtTransform(sounds[2].name, transform);
        Center.text = "Negative\nResistance";
        Module.HandlePass();
        yield return null;
    }

    void appendAnswer(int button)
    {
        if (!currentAnswer.Any()) currentAnswer.Add(button);
        else if (currentAnswer.Last() == button) currentAnswer.Remove(button);
        else if (currentAnswer.First() == button) {
            checkAnswer();
            return;
        }
        else if (currentAnswer.Contains(button) || currentAnswer.Count>5) return;
        else if ((((button ^ currentAnswer.Last()) - 1) & (button ^ currentAnswer.Last())) != 0)
        {
            //Debug.LogFormat("{0} xor {1} = {2}",button, currentAnswer.Last(), button ^ currentAnswer.Last());
            return;
        }
        else currentAnswer.Add(button);

        //string log = "";
        //foreach (int i in currentAnswer) log += "" + i + " ";
        //Debug.Log(log);
        Audio.PlaySoundAtTransform(sounds[0].name, transform);
        updateText();
    }
    
    
    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        ModuleId++;
        for (int i = 0; i < 1 << dimensions; i++)
        {
            positionVectors[i] = xyzFromNumber(i);
            colors[i] = new Color(
                (5 * ((i >>5) % 2) + 2*((i >>4) % 2) + 8*((i >>3) % 2) + 10*((i >>0) % 2)) / 25f,
                (8 * ((i >>5) % 2) + 5*((i >>4) % 2) + 2*((i >>3) % 2) + 10*((i >>1) % 2)) / 25f,
                (2 * ((i >>5) % 2) + 8*((i >>4) % 2) + 5*((i >>3) % 2) + 10*((i >>2) % 2)) / 25f
            );
            var i1 = i;
            spheres[i1].GetComponent<KMSelectable>().OnInteract += delegate
            {
                //Debug.Log("pressed "+i1);
                if (lockInput) return false; 
                if (!secondStage)
                {
                    lockInput = true;
                    secondStage = true;
                    Audio.PlaySoundAtTransform(sounds[1].name, transform);
                }
                else appendAnswer(i1);
                return false;
            };
        }
        initialize();
        StartCoroutine(move());
       
    }
    
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
        if (!Command.RegexMatch("^(([-+]{6})( |$))+"))
        {
            yield return "sendtochaterror Error";
        }
        else
        {
            foreach (var point in Command.Split(' '))
            {
                while (lockInput) yield return new WaitForSeconds(.3f);
                int x = 0;
                for (int i = 0; i < 6; i++) if (point[i] == '+') x += 1 << i;
                spheres[x].GetComponent<KMSelectable>().OnInteract();
                yield return new WaitForSeconds(.5f);
            }
        }
    }

    List<int> getAnswer()
    {
        int start = 0;
        if (readableConfig.Contains("-X")) start += 1;
        if (readableConfig.Contains("-Y")) start += 2;
        if (readableConfig.Contains("-Z")) start += 4;
        if (readableConfig.Contains("-W")) start += 8;
        if (readableConfig.Contains("-V")) start += 16;
        if (readableConfig.Contains("-U")) start += 32;
        List<int> ans = new List<int>();
        ans.Add(start);
        for (int i = 0; i < readableConfig.Length / 2; i++)
        {
            int offset;
            switch (readableConfig.Substring(i * 2, 2))
            {
                case "-X": offset = -1; break;
                case "-Y": offset = -2; break;
                case "-Z": offset = -4; break;
                case "-W": offset = -8; break;
                case "-V": offset = -16; break;
                case "-U": offset = -32; break;
                case "+X": offset = 1; break;
                case "+Y": offset = 2; break;
                case "+Z": offset = 4; break;
                case "+W": offset = 8; break;
                case "+V": offset = 16; break;
                case "+U": offset = 32; break;
                default: offset = 0; break;
            }
            ans.Add(ans.Last()+offset);
        }
        ans.Add(start);
        return ans;
    }
    
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        if (!secondStage) spheres[0].GetComponent<KMSelectable>().OnInteract();
        yield return new WaitForSeconds(0.2f);
        while (lockInput) yield return new WaitForSeconds(0.3f);
        while (currentAnswer.Any())
        { 
            spheres[currentAnswer.Last()].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
        List<int> ans = getAnswer();
        foreach (var point in ans)
        {
            spheres[point].GetComponent<KMSelectable>().OnInteract();
            yield return new WaitForSeconds(0.5f);
        }
    }
}
