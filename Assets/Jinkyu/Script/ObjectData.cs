using System;

public enum StaticPropertyType { None, Heavy, Light, Hard, Clear, Slippery }//πﬂ±§ æœ»Ê¿∫ ∏”∂Û«œ≥Î
public enum DynamicPropertyType { None, Elasticity, Reflection, Engine, Adhesion, Electricity , Fever }

[Serializable]
public class ObjectProperties
{
    public StaticPropertyType staticProperty;
    public DynamicPropertyType dynamicProperty; 
    
    public bool isInjected_Static;
    public bool isInjected_Dynamic;
}

[Serializable]
public class ObjectData
{
    public string name;
    public bool canHold;
    public ObjectProperties properties;

}
