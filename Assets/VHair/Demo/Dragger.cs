using UnityEngine;
using System.Collections;

public class Dragger : MonoBehaviour
{
    public Transform model;
    public float speed = 100;

    public void FixedUpdate()
    {
        if (!Input.GetMouseButton(0))
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        this.model.position = new Vector3
        (
            this.model.position.x + (-mouseX * this.speed * Time.fixedDeltaTime),
            this.model.position.y + (mouseY * this.speed * Time.fixedDeltaTime),
            this.model.position.z
        );
    }
}