using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance{
        get{
            return s_Instance;
        }
    }

    private static PlayerController s_Instance;

    private void Awake(){
        s_Instance = this;
    }
}
