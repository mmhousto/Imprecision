using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class SaveSystem : MonoBehaviour
    {
        /// <summary>
        /// Saves the player's data to a file in binary by serialization.
        /// </summary>
        /// <param name="player"></param>
        public static void SavePlayer(Player player)
        {
            if (CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.PS)
            {
#if UNITY_PS5
                PSSaveData.singleton.StartAutoSave();
#endif
            }
            else
            {
                BinaryFormatter formatter = new BinaryFormatter();
                string path = Application.persistentDataPath + "/playerData.hax";
                FileStream stream = new FileStream(path, FileMode.Create);

                SavePlayerData data = new SavePlayerData(player);

                formatter.Serialize(stream, data);
                stream.Close();
            }
        }

        /// <summary>
        /// Loads the player's data after deserializing it and returns it.
        /// </summary>
        /// <returns></returns>
        public static SavePlayerData LoadPlayer()
        {
            string path = Application.persistentDataPath + "/playerData.hax";
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                SavePlayerData data = formatter.Deserialize(stream) as SavePlayerData;
                stream.Close();

                return data;
            }
            else
            {
                Debug.Log("Save file not found in " + path);
                SavePlayerData data = new SavePlayerData();
                return data;
            }
        }

        public static void DeletePlayer()
        {
            if (CloudSaveLogin.Instance.currentSSO == CloudSaveLogin.ssoOption.PS)
            {
#if UNITY_PS5
                PSSaveData.singleton.DeleteSaveData();
#endif
            }
            else
            {
                string path = Application.persistentDataPath + "/playerData.hax";
                File.Delete(path);
            }

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}
