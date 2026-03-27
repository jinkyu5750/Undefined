using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystem : MonoBehaviour
{
    [SerializeField] private ObjectProperties properties;//저장 - 성질을 저장한 손은 해당 성질을 가지지 않는다. 보유만하는거임


    public bool isLookObject { get; private set; }
    [SerializeField] private float detectDistance;
    RaycastHit hit;

  

    private ObjectScript objectScript;


    private  ObjectScript liftedObject;

    private void Update()
    {
        DetectObject();
        Lifting();
    }
    public void DetectObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        if (Physics.Raycast(ray, out hit, detectDistance))
        {
            objectScript = hit.transform.GetComponent<ObjectScript>();
            if (objectScript != null)
            {
                ObjectData data = objectScript.GetData();
                if (data == null)
                {
                    UIManager.instance.OnOffDiscription(false);
                    return;
                }
                isLookObject = true;

                //디버깅용 디스크립션 패널
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
    public void Extraction(ObjectData targetData, bool isLeftClick)
    {
        if (targetData == null) return;

        if (isLeftClick)
        {
            if (properties.staticProperty != StaticPropertyType.None ||  // 현재가지고있는게없어야함
                targetData.properties.staticProperty == StaticPropertyType.None || //   None인 성질은 못뽑음
               targetData.properties.isInjected_Static) // 현재 성질이 주입된거는 안됨
                return;
            else
                properties.staticProperty = targetData.properties.staticProperty;
        }
        else
        {
            if (properties.dynamicProperty != DynamicPropertyType.None ||
                 targetData.properties.dynamicProperty == DynamicPropertyType.None ||
               targetData.properties.isInjected_Dynamic)
                return;
            else
                properties.dynamicProperty = targetData.properties.dynamicProperty;
        }
        UIManager.instance.SetPropertyText(properties);
    }


    public ObjectProperties Injection()
    {
        return properties;
    }
    public void Lifting()
    {

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (isLookObject && liftedObject == null)
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



    public ObjectScript GetObjectScript()
    {
        return objectScript;
    }
}
