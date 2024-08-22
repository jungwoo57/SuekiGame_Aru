using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("-----------------CORE")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("------------Object Pooling")]
    public GameObject donglePrefab;
    public Transform dongleGroup;
    public List<Dongle> donglePool;
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;
    public Dongle lastDongle;


    [Header("-----------------Audio")]
    public AudioSource[] sfxPlayer;
    public AudioSource mainBgmPlayer;
    public AudioSource startBgmPlayer;
    public AudioSource startAru;
    public AudioClip[] sfxClip;
    public enum Sfx { LevelUp, Next, Attach, Button, Over }
    int sfxCursor;
    

    [Header("-----------------UI")]
    public GameObject startGroup;
    public Text scoreText;
    public Text maxScoreText;
    public GameObject endGroup;

    [Header("-----------------ETC")]
    public GameObject line;
    public GameObject bottom;


    void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++) {
            MakeDongle();
        }

        if (!PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
        maxScoreText.text = "최고점수\n" + PlayerPrefs.GetInt("MaxScore").ToString();
    }
    void Start()
    {
        startAru.Play();
        startBgmPlayer.Play();
    }
   
    public void TocuhUp() 
    {
        if (lastDongle == null)
            return;
        lastDongle.Drop();
        lastDongle = null;
    }
    public void TouchDown() 
    {
        if (lastDongle == null)
            return;
        lastDongle.Drag();
    }
    void NextDongle()   
    {

        if (isOver) {
            return;
        }
        lastDongle = GetDongle();

        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext() 
    {
        while (lastDongle != null) 
        {
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);
        NextDongle();
    }

    public void GameStart() 
    {
        
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);
        startBgmPlayer.Stop();
        mainBgmPlayer.Play();
        SfxPlay(Sfx.Button);
        Invoke("NextDongle", 1.5f);
    }

    Dongle MakeDongle() {

        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle" + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);
        return instantDongle;
    }
    Dongle GetDongle()
    {
        for(int index = 0; index < donglePool.Count; index++) 
        {
            poolCursor = (poolCursor+1)%donglePool.Count;
            if (!donglePool[poolCursor].gameObject.activeSelf) 
            {
                return donglePool[poolCursor];
            }
        }
        return MakeDongle();
    }
    public void GameOver()
    {
        if (isOver)
            return;
        isOver = true;


        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine() 
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].rigid.simulated = false;
        }

        for (int index = 0; index < dongles.Length; index++)
        {
            dongles[index].Hide(Vector3.up * 1000);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);
        endGroup.SetActive(true);

        mainBgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine("ResetCoroutine");
    }

    IEnumerator ResetCoroutine() 
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Main");
    }
    public void SfxPlay(Sfx type) 
    {
        switch (type)
        {
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0,3)] ;
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
        }
        sfxPlayer[sfxCursor].Play();
        sfxCursor=(sfxCursor+1) % sfxPlayer.Length ;
        
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
    void LateUpdate()
    {
        scoreText.text = "현재점수\n" + score.ToString();
    }
}
