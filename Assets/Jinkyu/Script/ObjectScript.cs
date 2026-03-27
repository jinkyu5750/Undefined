using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    [SerializeField] private ObjectData data;

    [SerializeField] float injectTimer_Static = 5f;
    float curInjectTimer_Static;

    [SerializeField] float injectTimer_Dynamic = 5f;
    float curInjectTimer_Dynamic;


    private StaticPropertyType storedStaticProperty;
    private DynamicPropertyType storedDynamicProperty;

    private Transform player;

    public bool isLifted { get; private set; }

    float runningTime;

    [SerializeField] float speed=0.5f;
    [SerializeField] float length=0.5f;
    float InitYPos = 0f;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();
        InitYPos = transform.position.y;
    }
    //10초 뒤 돌아오는걸로
    private void Update()
    {

        if (isLifted)
        {
            Lifting();
        }

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
    public void SetProperties(ObjectProperties properties, bool isLeftClick) // Injection
    {
        if (properties == null) return;

        if (isLeftClick)
        {
            if (data.properties.isInjected_Static) return;

            data.properties.isInjected_Static = true;

            storedStaticProperty = data.properties.staticProperty; // 성질 임시저장
            data.properties.staticProperty = properties.staticProperty;
        }
        else
        {
            if (data.properties.isInjected_Dynamic) return;

            data.properties.isInjected_Dynamic = true;

            storedDynamicProperty = data.properties.dynamicProperty; // 성질 임시저장
            data.properties.dynamicProperty = properties.dynamicProperty;
        }

    }
    public ObjectData GetData()
    {
        return data;
    }

    public void Lifting()
    {

        runningTime += Time.deltaTime * speed;
        float yPos = (Mathf.Sin(runningTime) + 1f) * length;


        Vector3 targetPos = player.transform.forward * (transform.position - player.transform.position).magnitude;
        targetPos.y = InitYPos + yPos;
        transform.position = targetPos;


    }
    public void SetIsLifted(bool on)
    {
        isLifted = on;
    }
}