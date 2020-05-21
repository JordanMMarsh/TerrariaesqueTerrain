using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Camera cam;
    World world;

    private void Start()
    {
        cam = Camera.main;
        world = FindObjectOfType<World>();
    }

    //Add zooming out/in
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(GetRay(), out hit))
            {
                Debug.Log("Hit " + hit.transform.name);
                if (hit.transform.CompareTag("Backdrop"))
                {
                    world.InteractWithChunk(hit.point, 0);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(GetRay(), out hit))
            {
                if (hit.transform.CompareTag("Backdrop"))
                {
                    world.InteractWithChunk(hit.point, 2);
                }
            }
        }

        float cameraScrollSpeed = 20f;
        transform.position += new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * cameraScrollSpeed, Input.GetAxis("Vertical") * Time.deltaTime * cameraScrollSpeed, Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * cameraScrollSpeed * 50f);
    }

    private Ray GetRay()
    {
        return cam.ScreenPointToRay(Input.mousePosition);
    }
}
