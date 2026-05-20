using System;

public enum StaticPropertyType { None, Heavy, Light, Hard, Transparent, Slippery }//�߱� ������ �Ӷ��ϳ�
public enum DynamicPropertyType { None, Elasticity, Reflection, Engine, Propagation, Electricity, Fever }

[Serializable]
public class ObjectProperties
{
    public StaticPropertyType staticProperty;
    public DynamicPropertyType dynamicProperty;

    public bool isInjected_Static;
    public bool isInjected_Dynamic;
}