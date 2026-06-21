using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ObjectBase : MonoBehaviour, IPropertyReactor
{


    [SerializeField] private ObjectData data;

    private Rigidbody rig;
    private Renderer rend;

    [Header("주입 후 돌아오는 시간")]
    [SerializeField] float injectTimer_Static = 5f;
    [SerializeField] float injectTimer_Dynamic = 5f;
    float curInjectTimer_Static;
    float curInjectTimer_Dynamic;


    [SerializeField] private StaticPropertyType storedStaticProperty;
    private DynamicPropertyType storedDynamicProperty;

    private Transform player;
    [SerializeField] private CinemachineCamera cam;
    public bool isLifted { get; private set; }
    private bool readyToLift = false;

    [Header("오브젝트 들기 관련 ")]
    [SerializeField] float speed = 4;
    [SerializeField] float length = 0.1f;
    float runningTime;
    Vector3 targetPos;
    Vector3 offset;



    public virtual void OnPropertyInjected_Static(StaticPropertyType property)
    {
        switch (property)
        {
            case StaticPropertyType.Heavy:
                rig.mass = 100f;
                break;
            case StaticPropertyType.Light: // 들기가능으로?
                data.canHold = true;
                break;
            case StaticPropertyType.Transparent:
                rend.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 1f));
                StartCoroutine(SetAlpha(1f));
                break;

        }
    }
    public virtual void OnPropertyInjected_Dynamic(DynamicPropertyType property)
    {
        switch (property)
        {
            case DynamicPropertyType.Elasticity:
                //탄성높이기
                break;

        }
    }


    public virtual void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        cam = player.GetComponent<PlayerSystem>().cam;
        rig = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        targetPos = transform.position + new Vector3(0, 0.5f, 0);
    }


    private void Update()
    {

        if (isLifted)
            Lifting();

        InjectionTimer();

    }

    #region Injection
    public void InjectionTimer()
    {
        if (data.properties.isInjected_Static)
        {
            curInjectTimer_Static += Time.deltaTime;
            if (curInjectTimer_Static >= injectTimer_Static)
            {
                data.properties.staticProperty = storedStaticProperty;
                curInjectTimer_Static = 0;
                data.properties.isInjected_Static = false;
            }
        }


        if (data.properties.isInjected_Dynamic)
        {
            curInjectTimer_Dynamic += Time.deltaTime;
            if (curInjectTimer_Dynamic >= injectTimer_Dynamic)
            {
                data.properties.dynamicProperty = storedDynamicProperty;
                curInjectTimer_Dynamic = 0;
                data.properties.isInjected_Dynamic = false;
            }
        }
    }
    public bool SetProperties(ObjectProperties playerProperties, bool isLeftClick) // Injection
    {

        if (playerProperties == null) return false;

        if (isLeftClick)
        {
            if (data.properties.isInjected_Static) return false;
            data.properties.isInjected_Static = true;

            storedStaticProperty = data.properties.staticProperty; // ���� �ӽ�����
            data.properties.staticProperty = playerProperties.staticProperty;

            OnPropertyInjected_Static(data.properties.staticProperty);

        }
        else
        {
            if (data.properties.isInjected_Dynamic) return false;

            data.properties.isInjected_Dynamic = true;

            storedDynamicProperty = data.properties.dynamicProperty; // ���� �ӽ�����
            data.properties.dynamicProperty = playerProperties.dynamicProperty;
            OnPropertyInjected_Dynamic(data.properties.dynamicProperty);

        }

        return true;


    }
    #endregion

    #region Lifting
    public void Lifting()
    {

        runningTime += Time.deltaTime * speed;
        float yPos = (Mathf.Sin(runningTime)) * length;
        targetPos = player.position + (cam.transform.forward) * offset.magnitude;
        targetPos.y = targetPos.y + yPos + 1.5f;

        if (readyToLift)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
                readyToLift = false;

            return;
        }

        transform.position = targetPos;
    }
    public virtual void SetIsLifted(bool on)
    {
        isLifted = on;
        rig.useGravity = !on;
        if (on)
        {
            offset = transform.position - player.position; // 플레이어와의 거리
            runningTime = 0f;
            readyToLift = true;
        }

    }
    #endregion
    public ObjectData GetData()
    {
        return data;
    }

    public IEnumerator SetAlpha(float duration)
    {
        var mat = GetComponent<Renderer>().material;
        Texture baseMap = mat.GetTexture("_BaseMap");
        Color color = mat.color;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0.3f, time / duration);
            mat.color = color;
            yield return null;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && data.properties.dynamicProperty == DynamicPropertyType.Elasticity)
        {
            collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }
    }
}

