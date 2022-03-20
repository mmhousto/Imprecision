using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : MonoBehaviour
{

    public GameObject appleLogin, googleLogin;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        appleLogin.SetActive(false);
        googleLogin.SetActive(true);
#elif UNITY_IOS
        appleLogin.SetActive(true);
        googleLogin.SetActive(false);
#endif
    }

}
