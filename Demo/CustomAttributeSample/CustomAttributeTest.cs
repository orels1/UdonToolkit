using UdonSharp;

namespace UdonToolkit.Demo {
  public class CustomAttributeTest : UdonSharpBehaviour {
    [MyAttribute] public float someVariable;
  
    public bool anotherVariable;
  
    private void Start() {
    }
  }
}
