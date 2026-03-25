using UnityEngine;

public class PlayerSystem : MonoBehaviour
{
    [SerializeField] private ObjectProperties properties;//저장 - 성질을 저장한 손은 해당 성질을 가지지 않는다. 보유만하는거임


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
}
