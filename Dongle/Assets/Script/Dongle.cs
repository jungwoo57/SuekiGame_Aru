using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Dongle : MonoBehaviour
{
    public GameManager manager;
    public ParticleSystem effect;

    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    
    public Rigidbody2D rigid;
    Animator anim;
    CircleCollider2D circle;
    SpriteRenderer spriteRenderer;
    float deadTime;
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }
    void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float leftBorder = -4.64f + transform.localScale.x / 2;
            float rightBorder = 4.9f - transform.localScale.x / 2;
            if (mousePos.x < leftBorder)
                mousePos.x = leftBorder;
            else if (mousePos.x > rightBorder)
                mousePos.x = rightBorder;
            mousePos.z = 0;
            mousePos.y = 8;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }
    public void Drag()
    {
        isDrag = true;
    }
    public void Drop() 
    {
        isDrag = false;
        rigid.simulated = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine("AttachRoutine");
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
        {
            yield break;
        }
        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);
        yield return new WaitForSeconds(0.2f);
        isAttach = false;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Dongle") 
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();
            if(level == other.level && !isMerge && !other.isMerge && level <7)
            {
                //나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                //1.내가 아래에 있을때
                //2.동일한 높이일 때, 내가 오른쪽에 있을때
                if(meY < otherY || (meY==otherY && meX>otherX) ) 
                {//상대방 숨기고 본인은 래밸업
                    other.Hide(transform.position);
                    LevelUp();
                }
                

                //동글합치기
            }

        }
    }
    public void Hide(Vector3 targetPos)
    {
        isMerge = true;
        rigid.simulated = false;
        circle.enabled = false;
        if (targetPos == Vector3.up * 100)
            effect.Play();
        StartCoroutine(HideRoutine(targetPos));
    }
    IEnumerator HideRoutine(Vector3 targetPos) 
    {
        int frameCount = 0;
        while(frameCount < 20) 
        {
            frameCount++;
            if (targetPos != Vector3.up * 1000)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if(targetPos != Vector3.up * 1000) 
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero,0.2f);
            }
            yield return null;
        }
        manager.score += (int)Mathf.Pow(2, level);
        isMerge = false;
        gameObject.SetActive(false);
    }
    void LevelUp() 
    {
        isMerge = true;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine("LevelUpRoutine");
    }

    IEnumerator LevelUpRoutine()
    { 
        yield return new WaitForSeconds(0.2f);
        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);
        yield return new WaitForSeconds(0.3f);
        level++;

        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish") 
        {
            deadTime += Time.deltaTime;
            if(deadTime > 2)
            {
                spriteRenderer.color = Color.red;
            }
            if (deadTime > 5) 
            {
                manager.GameOver();
            }
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Finish") 
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }
    void EffectPlay() 
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
   
}
