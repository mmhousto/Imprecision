using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class MainManager : MonoBehaviour
    {

        public GameObject appleLogoutScreen;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void Logout()
        {
            if (CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.Apple)
            {
                appleLogoutScreen.SetActive(true);
            }
            else
            {
                CloudSaveLogin.Instance.Logout();
            }

                
        }

        public void PlayGame()
        {
            SceneLoader.LoadThisScene(2);
        }
    }
}
