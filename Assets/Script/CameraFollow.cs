using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //khai bao doi tuong ma camera se follow toi
    public Transform target;
    //Bien lam min hoat dong di chuyen cua camera theo nhan vat
    public float smoothing;

    Vector3 offset; // khoang cach tu camera toi nhan vat
    float lowY; //bien kiem soat camera khong di theo nhan vat khi roi khoi khung hinh

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - target.position;
        lowY = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetCamPost = target.position + offset;
        //Them luc day cho camera khi nhan vat di chuyen
        transform.position = Vector3.Lerp(transform.position, targetCamPost, smoothing * Time.deltaTime);

        if (transform.position.y < lowY)
        {
            transform.position = new Vector3(transform.position.x, lowY, transform.position.z);
        }
        if (transform.position.y > lowY)
        {
            transform.position = new Vector3(transform.position.x, lowY, transform.position.z);
        }
    }
}
