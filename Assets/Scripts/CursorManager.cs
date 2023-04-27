using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

namespace Com.MorganHouston.Imprecision
{
    public class CursorManager : MonoBehaviour
    {
        public StarterAssetsInputs playerInputs;
        public GameObject cursor;

        private void Update()
        {
            if(playerInputs != null)
            {
                UpdateCursor();
            }else if (cursor.activeInHierarchy)
            {
                cursor.SetActive(false);
            }
        }

        private void UpdateCursor()
        {
            if (GameManager.Instance?.isGameOver == true || playerInputs.aiming == false)
            {
                cursor.SetActive(false);
            }
            else if (playerInputs.aiming == true)
            {
                cursor.SetActive(true);
            }
        }
    }
}
