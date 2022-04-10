using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class Score : MonoBehaviour
{
    private static Score instance;

    public static Score Instance { get { return instance; } }

    static Vector3 center = new Vector3(0f, 2f, 0f);
    static Vector3 bullseye = new Vector3(0f, 0.1f, 0.1f);
    static Vector3 yellow = new Vector3(0, 0.3f, 0.3f);
    static Vector3 red = new Vector3(0f, 0.8f, 0.8f);
    static Vector3 blue = new Vector3(0f, 1.2f, 1.2f);
    static Vector3 black = new Vector3(0f, 1.6f, 1.6f);

    static Vector3 redPos = new Vector3(0f, 0.7f, 0.7f);
    static Vector3 bluePos = new Vector3(0f, 1.1f, 1.1f);
    static Vector3 blackPos = new Vector3(0f, 1.5f, 1.5f);
    public static int score;
    private TextMeshProUGUI scoreLbl;
    private static double absY, absZ;

    private void Awake()
    {
        if(instance != this && instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        scoreLbl = GetComponent<TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        scoreLbl.SetText("Score: {0}", score);
    }

    // Update is called once per frame
    void Update()
    {
        scoreLbl.SetText("Score: {0}", score);
    }

    public int GetHitPosition(Vector3 relPos)
    {
        var hitPos = relPos - center;

        // y axis up and down
        if (hitPos.y < 0)
        {
            absY = Mathf.Abs(hitPos.y);
        }
        else if (hitPos.y > 0)
        {
            absY = hitPos.y;
            red.y = redPos.y;
            blue.y = bluePos.y;
            black.y = blackPos.y;
        }
        else
        {
            absY = hitPos.y;
        }

        // z axis left and right
        if (hitPos.z < 0)
        {
            absZ = Mathf.Abs(hitPos.z);
        }
        else if (hitPos.z > 0)
        {
            absZ = hitPos.z;
            red.z = redPos.z;
            blue.z = bluePos.z;
            black.z = blackPos.z;
        }
        else
        {
            absZ = hitPos.z;
        }

        Debug.Log(hitPos);
        if (absY <= bullseye.y && absZ <= bullseye.z)
        {
            Debug.Log("Bullseye");
            AddPoints(200);
            return 0;
        }
        else if (absY <= yellow.y && absZ <= yellow.z && absY > 0 && absZ > 0)
        {
            Debug.Log("Yellow");
            AddPoints(90);
            return 1;
        }
        else if (absY <= red.y && absZ <= red.z)
        {
            Debug.Log("Red");
            AddPoints(70);
            return 2;
        }
        else if (absY <= blue.y && absZ <= blue.z)
        {
            Debug.Log("Blue");
            AddPoints(50);
            return 3;
        }
        else if (absY <= black.y && absZ <= black.z)
        {
            Debug.Log("Black");
            AddPoints(30);
            return 4;
        }
        else
        {
            Debug.Log("White");
            AddPoints(10);
            return 5;
        }
    }

    public void AddExtraPoints()
    {
        score += 50;
    }

    public void AddPoints(int points)
    {
        score += points;
    }
}
