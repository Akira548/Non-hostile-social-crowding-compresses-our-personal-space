using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;
using System;
using UnityEngine.UI;
using MORPH3D;

public class Exp4a : MonoBehaviour
{
    public int repeat = 1;
    private string path = "Restults/";
    private float[,] Trials;
    private GameObject controller;//调用VR控制器组件
    private int trial_current = 0;
    public int Parictice_Trial = 0;
    private Animator mcs_animator;
    static GameObject mcs;
    public GameObject[] male;
    public GameObject[] female;
    public GameObject[] crowd;
    public GameObject[] UI;
    private float walkingSpeed;
    public float IT = 2;
    private float trial_total;
    public string current_state;
    private bool temp_trigger = false;
    private bool isStop, Stop;
    private Vector3 pos_camera, pos_person;
    static M3DCharacterManager m3dc;
    private bool sytex;
    private bool infoMessage = false;
    private int loopNum;
    private int start;
    private float Practice_Time = 20;
    private int SubNum = 0;
    private int sex = 0;
    private int handedness = 0;
    private int age = 0;
    private int factor_number;
    private int Practice_current = 0;
    private List<float[]> crowd_pos;
    private DateTime dt = DateTime.Now;
    private bool start_practice;
    private bool start_experiment;
    private int RandomIndex;
    private string last_state;
    private bool hastip = false;
    private bool can_respond = false;
    private float StartTime;
    private bool D1 = false;
    private float D1time;
    public string Experiment;
    public GameObject[] Mob_pic;
    public GameObject[] Swat_pic;
    private GameObject[] Pic;
    private int pic_page = 0;
    private string condition;

    // Use this for initialization
    void Start()
    {
        GetComponent<UpsyVR_ExpFactor_Generator>().setVal();
        controller = GameObject.Find("UpsyVR_VRController");
        float[,] trial_conbinefactor = UpsyVR_ExpFactor_Generator.ExpFactor_Generator();
        Trials = TiralGenerater(UpsyVR_ExpFactor_Generator.ExpFactor_Generator(), repeat);
        trial_total = trial_conbinefactor.GetLength(0) * repeat;
        factor_number = trial_conbinefactor.GetLength(1);
        current_state = "Starting";
        //crowd_pos = avaliable_position();
        can_respond = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (current_state == "WaitPractice")
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                pic_page++;
                if (pic_page >= Pic.GetLength(0))
                {
                    StartTime = Time.time;
                    Practice_Initialized();
                    Pic[pic_page - 1].SetActive(false);
                    current_state = "Practice";
                }
                else
                {
                        Pic[pic_page - 1].SetActive(false);
                        Pic[pic_page].SetActive(true);
                }
            }
        }


        if (current_state == "Practice")
        {
            if (mcs)
            {
                mcs.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            ///////////////    UI    ///////////////
            UI[4].SetActive(true);
            UI[0].GetComponent<Text>().text = "====Practice====";
            UI[1].GetComponent<Text>().text = (Practice_current + 1).ToString() + "  / " + Parictice_Trial.ToString();
            UI[2].GetComponent<Text>().text = "Density：" + Trials[RandomIndex, 1].ToString() + " 人";
            UI[3].GetComponent<Toggle>().isOn = controller.GetComponent<UpsyVR_ControllerEvents>().Right_Trigger;
            ////////////////////////////////////////

            if (controller.GetComponent<UpsyVR_ControllerEvents>().Right_Trigger && !temp_trigger && !Stop && can_respond && Time.time - StartTime > 6)//最小反映间隔6s
            {


                if (!D1)
                {
                    pos_person = mcs.transform.position;
                    mcs_animator = mcs.GetComponent<Animator>();
                    StartCoroutine(D1_2_D2());
                    D1 = true;
                    D1time = Time.time;
                }
                else
                {
                    if (Time.time - D1time > 1f)
                    {
                        D1 = false;
                        can_respond = false;
                        start_practice = false;
                        temp_trigger = true;
                        isStop = true; //先让人物停下脚步
                        pos_person = mcs.transform.position;
                        walkingSpeed = 0; ////先让人物停下脚步
                        mcs_animator = mcs.GetComponent<Animator>();
                        mcs_animator.SetFloat("walking", walkingSpeed);
                        Practice_current++;
                        if (Practice_current < Parictice_Trial)
                        {
                            StartTime = Time.time;
                            StartCoroutine(preReStart());//开始下一次实验
                        }
                        else
                        {
                            //Stop = true; //实验完成
                            if (!hastip) //提示练习结束，准备开始
                            {
                                start = Messagebox_Button_MCS.MessageBox(IntPtr.Zero, "练习结束,开始正式实验。", "提示", 1);
                                hastip = true;
                                if (start == 1)
                                {
                                    StartTime = Time.time;
                                    StopCoroutine("CountDown");
                                    Experiment_Initialized();
                                    current_state = "Experiment";
                                }
                                else
                                {
                                    hastip = false;
                                }

                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (current_state == "Experiment")
            {
                if (mcs)
                {
                    mcs.transform.rotation = Quaternion.Euler(0, 0, 0);
                }                ///////////////    UI    ///////////////
                UI[4].SetActive(true);
                UI[0].GetComponent<Text>().text = "====Experiment====";
                UI[1].GetComponent<Text>().text = (trial_current + 1).ToString() + "  / " + trial_total.ToString();
                UI[2].GetComponent<Text>().text = "Density："+Trials[trial_current, 1].ToString() + " 人 ";
                UI[3].GetComponent<Toggle>().isOn = controller.GetComponent<UpsyVR_ControllerEvents>().Right_Trigger;
                ////////////////////////////////////////
                if (controller.GetComponent<UpsyVR_ControllerEvents>().Right_Trigger && !temp_trigger && !Stop && can_respond && Time.time - StartTime > 6)
                {
                    //////////////////////    D1 measurement ///////////////////////////
                    if (!D1)
                    {
                        mcs_animator = mcs.GetComponent<Animator>();
                        pos_person = mcs.transform.position;
                        StartCoroutine(D1_2_D2());
                        Trials[trial_current, factor_number] = -pos_person.z;
                        D1 = true;
                        D1time = Time.time;
                    }
                    else
                    {
                        if (Time.time - D1time > 1f)
                        {
                            D1 = false;

                            //////////////////////    D2 measurement ///////////////////////////
                            can_respond = false;
                            start_experiment = false;
                            temp_trigger = true;
                            isStop = true; //先让人物停下脚步
                            pos_person = mcs.transform.position;
                            walkingSpeed = 0; ////先让人物停下脚步
                            mcs_animator = mcs.GetComponent<Animator>();
                            mcs_animator.SetFloat("walking", walkingSpeed);
                            Trials[trial_current, factor_number + 1] = -pos_person.z;
                            trial_current++;
                            if (trial_current < trial_total)
                            {
                                Txt_data(path, Trials);
                                if (trial_current == 70)
                                {
                                    start = Messagebox_Button_MCS.MessageBox(IntPtr.Zero, "休息一分钟，休息结束后单击确定继续实验。", "提示", 1);
                                }
                                StartTime = Time.time;
                                StartCoroutine(preReStart());//开始下一次实验
                            }
                            else
                            {
                                Stop = true; //实验完成
                                Txt_data(path, Trials);
                                current_state = "End";
                                UI[0].GetComponent<Text>().text = "====End====";
                                start = Messagebox_Button_MCS.MessageBox(IntPtr.Zero, "实验结束", "提示", 1);
                                if (start == 1)
                                {
                                    Application.Quit();
                                }
                                else
                                {
                                    Application.Quit();
                                }

                            }
                        }
                    }
                }
            }
        }
        last_state = current_state;
        temp_trigger = controller.GetComponent<UpsyVR_ControllerEvents>().Right_Trigger;
    }

    void Resetart()
    {
        walkingSpeed = 1.0f;//行走速度为1，因为前面控制人物停止行动的时候设置为0了
        isStop = false;
        sytex = true;
        foreach (GameObject a in crowd)
        {
            a.SetActive(false);
        }
        int[] random_crowd = RandomSequence(crowd.GetLength(0));
        foreach (GameObject a in crowd)
        {
            a.SetActive(false);
        }
        if (current_state == "Practice")
        {
            RandomIndex = UnityEngine.Random.Range(0, (int)trial_total);
            crowd_pos = new List<float[]>();
            crowd_pos = avaliable_position((int)Trials[RandomIndex, 1]);
            for (int i = 0; i < (int)Trials[RandomIndex, 1]; i++)
            {
                crowd[random_crowd[i]].SetActive(true);
                crowd[random_crowd[i]].transform.position = new Vector3(crowd_pos[i][0], crowd[random_crowd[i]].transform.position.y, crowd_pos[i][1]);
                crowd[random_crowd[i]].transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            }
            StartCoroutine(CountDown(Trials, RandomIndex));
        }
        else
        {
            if (current_state == "Experiment")
            {
                crowd_pos = new List<float[]>();
                crowd_pos = avaliable_position((int)Trials[trial_current, 1]);
                for (int i = 0; i < (int)Trials[trial_current, 1]; i++)
                {
                    crowd[random_crowd[i]].SetActive(true);
                    crowd[random_crowd[i]].transform.position = new Vector3(crowd_pos[i][0], crowd[random_crowd[i]].transform.position.y, crowd_pos[i][1]);
                    crowd[random_crowd[i]].transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                }
                StartCoroutine(CountDown(Trials, trial_current));
            }

        }
    }

    void Practice_Initialized()
    {
        foreach (GameObject a in crowd)
        {
            a.SetActive(false);
        }
        can_respond = false;
        walkingSpeed = 1.0f;//行走速度为1，因为前面控制人物停止行动的时候设置为0了
        isStop = false;
        sytex = true;
        int[] random_crowd = RandomSequence(crowd.GetLength(0));
        RandomIndex = UnityEngine.Random.Range(0, (int)trial_total);
        crowd_pos = new List<float[]>();
        crowd_pos = avaliable_position((int)Trials[RandomIndex, 1]);
        for (int i = 0; i < (int)Trials[RandomIndex, 1]; i++)
        {
            crowd[random_crowd[i]].SetActive(true);
            crowd[random_crowd[i]].transform.position = new Vector3(crowd_pos[i][0], crowd[random_crowd[i]].transform.position.y, crowd_pos[i][1]);
            crowd[random_crowd[i]].transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        }
        StartCoroutine(CountDown(Trials, RandomIndex));
    }

    void Experiment_Initialized()
    {
        foreach (GameObject a in crowd)
        {
            a.SetActive(false);
        }
        start = 0;
        can_respond = false;
        walkingSpeed = 1.0f;//行走速度为1，因为前面控制人物停止行动的时候设置为0了
        isStop = false;
        sytex = true;
        int[] random_crowd = RandomSequence(crowd.GetLength(0));
        crowd_pos = new List<float[]>();
        crowd_pos = avaliable_position((int)Trials[trial_current, 1]);
        for (int i = 0; i < (int)Trials[trial_current, 1]; i++)
        {
            crowd[random_crowd[i]].SetActive(true);
            crowd[random_crowd[i]].transform.position = new Vector3(crowd_pos[i][0], crowd[random_crowd[i]].transform.position.y, crowd_pos[i][1]);
            crowd[random_crowd[i]].transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        }
        StartCoroutine(CountDown(Trials, 0));
    }

    private IEnumerator preReStart()
    {
        can_respond = false;
        yield return new WaitForSeconds(IT);//休息relaxTime的时间
        Resetart();
    }
    private IEnumerator D1_2_D2()
    {
        mcs_animator = mcs.GetComponent<Animator>();
        mcs_animator.speed = 0;
        yield return new WaitForSeconds(1f);//休息relaxTime的时间
        mcs_animator.speed = 1f;
        //mcs_animator.SetFloat("walking", 1);
    }

    IEnumerator CountDown(float[,] Trials, int conut) //走路
    {
        foreach (GameObject a in male)
        {
            a.SetActive(false);
            a.transform.position = new Vector3(0, 0.018f, -8);
        }
        foreach (GameObject a in female)
        {
            a.SetActive(false);
            a.transform.position = new Vector3(0, 0.018f, -8);
        }
        yield return new WaitForSeconds(5);//观察场景
        if (Trials[conut, 0] == 1)
        {
            mcs = male[UnityEngine.Random.Range(0, 5)];
            mcs.SetActive(true);
        }
        else
        {
            mcs = female[UnityEngine.Random.Range(0, 5)];
            mcs.SetActive(true);
        }
        float distance = Trials[conut, 2];//get angle from trials
        mcs_animator = mcs.GetComponent<Animator>();
        //while (isStop == false)
        //{
        mcs_animator.SetFloat("walking", walkingSpeed);//以walkingSpeed的速度行走
        can_respond = true;
        //}
    }

    public static float[,] TiralGenerater(float[,] ConbainFactor, int repeat)
    {
        int repeatindex = 0;
        //////////////////   生成trial  ///////////////////
        float[,] temparr = new float[(ConbainFactor.GetLength(0)) * repeat, (ConbainFactor.GetLength(1) + 2)];
        float[,] shuffearr = Shuffle(ConbainFactor);
        /////////////////////////////////
        for (int i = 0; i < repeat; ++i)
        {
            for (int k = 0; k < ConbainFactor.GetLength(0); ++k)
            {
                for (int j = 0; j < ConbainFactor.GetLength(1); j++)
                {
                    temparr[repeatindex * shuffearr.GetLength(0) + k, j] = shuffearr[k, j];
                }
                temparr[repeatindex * shuffearr.GetLength(0) + k, ConbainFactor.GetLength(1)] = -1;
                temparr[repeatindex * shuffearr.GetLength(0) + k, ConbainFactor.GetLength(1) + 1] = -1;

            }
            repeatindex++;
            shuffearr = Shuffle(ConbainFactor);
        }
        return temparr;
    }

    public static float[,] Shuffle(float[,] a)
    {
        /////////////   二维数组打乱第一列  ///////////////////
        float[,] temparr = new float[(a.GetLength(0)), (a.GetLength(1))];
        /////////////生成随机数///////////
        int[] randomindex = new int[a.GetLength(0)];
        randomindex = RandomSequence(a.GetLength(0));
        /////////////////////////////////
        for (int i = 0; i < a.GetLength(0); ++i)
        {
            for (int j = 0; j < a.GetLength(1); j++)
            {
                temparr[i, j] = a[(randomindex[i]), j];
            }
        }
        return temparr;
    }
    public static int[] RandomSequence(int total)
    {
        /////////////   生成不重复数字  ///////////////////
        int[] hashtable = new int[total];
        int[] output = new int[total];

        UnityEngine.Random random = new UnityEngine.Random();
        for (int i = 0; i < total; i++)
        {
            int num = UnityEngine.Random.Range(0, total);
            while (hashtable[num] > 0)
            {
                num = UnityEngine.Random.Range(0, total);
            }

            output[i] = num;
            hashtable[num] = 1;
        }
        return output;
    }

    public void SubNum_(string newname)
    {
        SubNum = int.Parse(newname);
    }
    public void sex_(int newsex)
    {
        sex = newsex;
    }
    public void handedness_(int newhandedness)
    {
        handedness = newhandedness;
    }
    public void age_(string newage)
    {
        age = int.Parse(newage);
    }
    public void Ready()
    {
        GameObject.Find("StartMeau").SetActive(false);
        current_state = "Practice";
        StartTime = Time.time;
        Practice_Initialized();
    }


    void Txt_data(string path, float[,] a)
    {
        path = path + Experiment + "/";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string FILE_NAME = path + "Sub_" + SubNum + ".txt";
        StreamWriter sr;
        sr = File.CreateText(FILE_NAME);
        sr.Write("Experiment");
        sr.Write("\t");
        sr.Write("Sub_Num");
        sr.Write("\t");
        sr.Write("Age");
        sr.Write("\t");
        sr.Write("Sex");
        sr.Write("\t");
        sr.Write("Handness");
        sr.Write("\t");
        sr.Write("Condition");
        sr.Write("\t");
        for (int i = 0; i < factor_number; i++)
        {
            sr.Write(UpsyVR_ExpFactor_Generator.factor_name[i]);
            sr.Write("\t");
        }
        sr.Write("D1");
        sr.Write("\t");
        sr.Write("D2");
        sr.Write("\n");
        for (int i = 0; i < a.GetLength(0); ++i)
        {
            sr.Write("Crowding_" + dt.ToString("d"));
            sr.Write("\t");
            sr.Write("Sub_" + SubNum.ToString());
            sr.Write("\t");
            sr.Write(age.ToString());
            sr.Write("\t");
            if (sex == 0)
            {
                sr.Write("Female");
                sr.Write("\t");
            }
            else
            {
                sr.Write("Male");
                sr.Write("\t");
            }
            sr.Write(handedness.ToString());
            sr.Write("\t");
            sr.Write(condition);
            sr.Write("\t");
            for (int j = 0; j < factor_number; j++)
            {
                sr.Write(a[i, j]);
                sr.Write("\t");
            }
            sr.Write(a[i, factor_number].ToString());
            sr.Write("\t");
            sr.Write(a[i, factor_number + 1].ToString());
            sr.Write("\n");
        }
        sr.Close();
    }


    public void Mob()
    {
        current_state = "WaitPractice";
        Pic = Mob_pic;
        condition = "Mob";
        Pic[pic_page].SetActive(true);
    }
    public void Swat()
    {
        current_state = "WaitPractice";
        Pic = Swat_pic; ;
        condition = "Swat";
        Pic[pic_page].SetActive(true);
    }
    static List<float[]> avaliable_position(int people)
    {
        List<float> row_L = new List<float>();
        for (float i = -3.6f; i <= -0.4f; i = i + 0.4f)
        {
            row_L.Add(i);
        }
        List<float> row_R = new List<float>();
        for (float i = 0.4f; i <= 3.6f; i = i + 0.4f)
        {
            row_R.Add(i);
        }
        List<float> col = new List<float>();
        for (float i = -7.6f; i <= -0.4f; i = i + 0.4f)
        {
            col.Add(i);
        }
        List<float[]> pos_L = new List<float[]>();
        for (int m = 0; m < row_L.Count; m++)
        {
            for (int n = 0; n < col.Count; n++)
            {
                pos_L.Add(new float[] { row_L[m], col[n] });
            }
        }
        List<float[]> pos_R = new List<float[]>();
        for (int m = 0; m < row_R.Count; m++)
        {
            for (int n = 0; n < col.Count; n++)
            {
                pos_R.Add(new float[] { row_R[m], col[n] });
            }
        }
        List<float[]> pos = new List<float[]>();
        int[] RL = RandomSequence(pos_L.Count);
        for (int a = 0; a < (people / 2); a++)
        {
            pos.Add(pos_L[RL[a]]);
        }
        int[] RR = RandomSequence(pos_R.Count);
        for (int b = 0; b < (people / 2); b++)
        {
            pos.Add(pos_R[RR[b]]);
        }
        return pos;
    }

}
