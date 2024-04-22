using Com.MorganHouston.Imprecision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jewel : MonoBehaviour
{
    public AudioClip pickupSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(new Vector3(0f, 1.5f, 0f), Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<AudioSource>().PlayOneShot(pickupSound, 1f);
            Player.Instance.GainJewels(1);
            Score.Instance.AddPoints(500);
            Destroy(gameObject);
        }
    }
}
