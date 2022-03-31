using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {
    [SerializeField]
    private int speed = 5;

    public Transform root;
    private GameObject player;

    void Start() {
        player = GameObject.FindWithTag("Player");
        speed = Random.Range(5, 26);

    }

    void Update() {
        root.transform.RotateAround(player.transform.position, Vector3.up, Time.deltaTime * speed);
        
    }

    void LateUpdate() {
        transform.parent.transform.LookAt(player.transform);

    }

    void OnCollisionEnter(Collision col) {
        if (col.gameObject.tag == "arrow") {
            Vector3 relPos = gameObject.transform.InverseTransformPoint(col.transform.position);
            float linePosX = col.transform.localPosition.x;
            float linePosY = col.transform.localPosition.y;
            //Score.AddPoints(relPos);
            Destroy(transform.root.gameObject);

        }
    }
}
