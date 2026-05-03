using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public interface IPropertyReactor
{
    bool CanReact(PropertyType property);
    void OnPropertyInjected(PropertyType property);
}
