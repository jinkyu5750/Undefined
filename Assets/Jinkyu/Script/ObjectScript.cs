using UnityEngine;

public class ObjectScript : MonoBehaviour
{


    [SerializeField] private ObjectData data;

    private Rigidbody rig;
    [Header("주입 후 돌아오는 시간")]
    [SerializeField] float injectTimer_Static = 5f;
    [SerializeField] float injectTimer_Dynamic = 5f;
    float curInjectTimer_Static;
    float curInjectTimer_Dynamic;


   [SerializeField] private StaticPropertyType storedStaticProperty;
    private DynamicPropertyType storedDynamicProperty;

    private Transform player;

    public bool isLifted { get; private set; }
    private bool readyToLift = false;

    [Header("오브젝트 들기 관련 ")]
    [SerializeField] float speed = 4;
    [SerializeField] float length = 0.1f;
    float runningTime;
    Vector3 targetPos;
    Vector3 offset;




    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        rig = GetComponent<Rigidbody>();
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
    public void SetProperties(ObjectProperties playerProperties, bool isLeftClick) // Injection
    {

        if (playerProperties == null) return;

        if (isLeftClick)
        {
            if (data.properties.isInjected_Static) return;
            data.properties.isInjected_Static = true;

            storedStaticProperty = data.properties.staticProperty; // ���� �ӽ�����
            data.properties.staticProperty = playerProperties.staticProperty;
        }
        else
        {
            if (data.properties.isInjected_Dynamic) return;

            data.properties.isInjected_Dynamic = true;

            storedDynamicProperty = data.properties.dynamicProperty; // ���� �ӽ�����
            data.properties.dynamicProperty = playerProperties.dynamicProperty;
        }


    }
    #endregion

    #region Lifting
    public void Lifting()
    {

        runningTime += Time.deltaTime * speed;
        float yPos = (Mathf.Sin(runningTime)) * length;
        targetPos = player.position + (Camera.main.transform.forward) * offset.magnitude;
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
    public void SetIsLifted(bool on)
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


}