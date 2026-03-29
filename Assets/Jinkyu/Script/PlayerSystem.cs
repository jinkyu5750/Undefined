using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerSystem : MonoBehaviour
{
    [Header("현재 보유중인 성질")]
    [SerializeField] private ObjectProperties properties;//���� - ������ ������ ���� �ش� ������ ������ �ʴ´�. �������ϴ°���

    [Header("오브젝트 감지 거리")]
    [SerializeField] private float detectDistance;

    public bool isLookObject { get; private set; }
    RaycastHit hit;
    private ObjectScript objectScript;
    private ObjectData data;
    private ObjectScript liftedObject;

    [Header("던지기 관련")]
    [SerializeField] float throwPower;
    [SerializeField] float throwTime = 2f;
    float throwTimer = 0;


    ButtonControl input = null;
    bool isClickObject;
    [SerializeField] private float holdTime = 2f;
    [SerializeField] private float holdTimer = 0;
    [SerializeField] private bool isLeftClick;
    [SerializeField] private bool isActionDone;

    private void Update()
    {
        DetectObject();
        Lifting();
        Throw();
        Extraction_Injection();

    }
    public void DetectObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        if (Physics.Raycast(ray, out hit, detectDistance))
        {
            objectScript = hit.transform.GetComponent<ObjectScript>();
            if (objectScript != null)
            {
                data = objectScript.GetData();
                if (data == null)
                {
                    UIManager.instance.OnOffDiscription(false);
                    return;
                }
                isLookObject = true;

                //������ ��ũ���� �г�
                UIManager.instance.OnOffDiscription(true);
                UIManager.instance.SetObjectDiscription(
                    data.name,
                    data.properties.staticProperty.ToString(),
                    data.properties.dynamicProperty.ToString()
                );
                return;
            }
        }

        isLookObject = false;

    }

    public void Extraction_Injection()
    {
        if (liftedObject != null) return;

        if (!isLookObject)
        {
            holdTimer = 0;
            UIManager.instance.OnOff_Slider(false);
            isClickObject = false;
            return;
        }

        if (Mouse.current.press.wasPressedThisFrame)
        {
            isLeftClick = Mouse.current.leftButton.wasPressedThisFrame;
            input = isLeftClick ? Mouse.current.leftButton : Mouse.current.rightButton;
            isClickObject = true;
        }

        if (input != null && input.wasReleasedThisFrame)
        {
            isActionDone = false;
            holdTimer = 0;
            UIManager.instance.OnOff_Slider(false);
        }



        if (input != null && input.isPressed)
        {
            if (isActionDone || !isClickObject) return;

            holdTimer += Time.deltaTime;
            UIManager.instance.FillSlider(holdTimer, holdTime, false, isLeftClick);

            if (holdTimer >= holdTime)
            {
                bool extractionOrInjection = isLeftClick ? properties.staticProperty == StaticPropertyType.None : properties.dynamicProperty == DynamicPropertyType.None;
                //좌클릭이면 정적 none체크, 우클릭이면 동적 none체크
                if (extractionOrInjection)
                    Extraction(data, isLeftClick);
                else
                    Injection(properties, isLeftClick);

                isActionDone = true;
                holdTimer = 0;
                UIManager.instance.OnOff_Slider(false);
            }
        }

    }

    public void Extraction(ObjectData targetData, bool isLeftClick)
    {
        if (targetData == null) return;



        if (isLeftClick)
        {
            if (targetData.properties.staticProperty == StaticPropertyType.None || // 오브젝트는 성질이 있어야 함
               targetData.properties.isInjected_Static) // 주입된 성질이면 안됨
                return;
            else
                properties.staticProperty = targetData.properties.staticProperty;
        }
        else
        {
            if (targetData.properties.dynamicProperty == DynamicPropertyType.None ||
          targetData.properties.isInjected_Dynamic)
                return;
            else
                properties.dynamicProperty = targetData.properties.dynamicProperty;
        }
        UIManager.instance.SetPropertyText(properties);
    }

    public void Injection(ObjectProperties playerProperties, bool isLeftClick)
    {
        objectScript.SetProperties(playerProperties, isLeftClick);
    }
    public void Lifting()
    {

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isLookObject && liftedObject == null && hit.collider != null)
            {
                liftedObject = hit.transform.GetComponent<ObjectScript>();
                liftedObject.SetIsLifted(true);
            }
            else if (liftedObject.isLifted)
            {
                liftedObject.SetIsLifted(false);
                liftedObject = null;
            }
        }


    }

    public void Throw()
    {
        if (liftedObject == null) return;

        if (Keyboard.current.fKey.isPressed)
        {
            throwTimer += Time.deltaTime;
            UIManager.instance.FillSlider(throwTimer, throwTime, true);
        }


        if (Keyboard.current.fKey.wasReleasedThisFrame)
        {

            if (throwTimer >= throwTime)
            {
                liftedObject.SetIsLifted(false);
                liftedObject.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * throwPower, ForceMode.Impulse);
                liftedObject = null;
            }
            throwTimer = 0f;
            UIManager.instance.OnOff_Slider(false);
        }


    }

    public ObjectScript GetObjectScript()
    {
        return objectScript;
    }
}
