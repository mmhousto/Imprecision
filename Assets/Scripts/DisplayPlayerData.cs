using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisplayPlayerData : MonoBehaviour
{

    public TextMeshProUGUI userNameTxt, pointsTxt, levelTxt;
    public Slider xpSlider;

    public Player player;

    private int xp, points, level;
    private string userName;

    // Start is called before the first frame update
    void Start()
    {
        xpSlider.minValue = 0;
        xpSlider.maxValue = 150 * level;
    }

    // Update is called once per frame
    void Update()
    {
        if(xp != player.userXP)
            xp = player.userXP;

        if (points != player.userPoints)
            points = player.userPoints;

        if (level != player.userLevel)
        {
            level = player.userLevel;
            xpSlider.maxValue = 150 * level;
        }

        if (userName != player.userName)
            userName = player.userName;

        userNameTxt.text = userName;
        pointsTxt.text = $"Points: {points}";
        levelTxt.text = $"Level: {level}";
        xpSlider.value = xp;
    }




}
