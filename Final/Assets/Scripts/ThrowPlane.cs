using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThrowPlane : MonoBehaviour
{
    public static ThrowPlane Instance;
    
    private float startTime, endTime, swipeDistance, swipeTime;

    private Vector2 startMousePos, endMousePos;   //mouse pos
    public float minSwipeDistance = 0;

    private float planeSpd = 0;
    private float maxPlaneSpd = 230;
    private Vector3 angle;
    
    private Vector3 newPlanePos, resetPlanePos;
    private Quaternion resetPlaneRotation;
    private Vector3 resetCameraPos;
    private Rigidbody rb;

    public bool flying = false;
    public bool fell = false;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        resetPlanePos = transform.position;
        resetPlaneRotation = transform.rotation;
        resetCameraPos = Camera.main.transform.position;
        // ResetPlane();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, -12f, 7f),
            transform.position.z);
        if (transform.position.y <= -10f)
        {
            fell = true;
        }

        if (fell)
        {
            Invoke("ResetPlane", 5f);
            fell = false;
        }
        
    }

    private void FixedUpdate()
    {
        // If at level 1, enable keyboard control
        if (GameManager.Instance.LevelCounter >= 1)
        {
            if (Input.GetKey(KeyCode.W))
            {
                // Debug.Log("going up");
                FlyUpward();
            }

            if (Input.GetKey(KeyCode.S))
            {
                // Debug.Log("going down");
                FlyDownward();
            }
        }
    }

    void FlyUpward()
    {
        rb.AddForce(new Vector3(0, 25f, 0));
    }

    void FlyDownward()
    {
        rb.AddForce(new Vector3(0, -20f, 0));
    }

    void ResetPlane()
    {
        angle = Vector3.zero;
        endMousePos = Vector2.zero;
        startMousePos = Vector2.zero;
        planeSpd = 0;
        startTime = 0;
        endTime = 0;
        swipeDistance = 0;
        swipeTime = 0;

        gameObject.SetActive(true);
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        transform.position = resetPlanePos;
        transform.rotation = resetPlaneRotation;
        Camera.main.transform.position = resetCameraPos;

        flying = false;

        if (GameManager.Instance.LevelCounter == 2)
        {
            GameManager.Instance.ScoreCounter = 0;
            
            // Reset spawn-related state
            GameManager.Instance.spawned = false;
        
            // Destroy left collectibles
            int nbCollectibles = GameManager.Instance.collectibles.Count; 
            if (nbCollectibles > 0)
            {
                for (var i = nbCollectibles - 1; i >= 0; i--)
                {
                    Destroy(GameManager.Instance.collectibles[i].gameObject);
                    GameManager.Instance.collectibles.RemoveAt(i);
                }
            }
        }
        
        // GameManager.Instance.beginText.gameObject.SetActive(true);
        // GameManager.Instance.beginText.text = "Let's try again";
    }

    private void OnMouseDown()
    {
        startTime = Time.time;
        startMousePos = Input.mousePosition;
        
        if (GameManager.Instance.gameObject.activeSelf)
        {
            // Debug.Log("remove text");
            
            // Arrow
            if (GameManager.Instance.arrowImg.gameObject.activeSelf)
            {
                GameManager.Instance.arrowImg.gameObject.SetActive(false);
                Debug.Log("arrow");
            }
            
            // Begin text
            if (GameManager.Instance.beginText.gameObject.activeSelf)
            {
                GameManager.Instance.beginText.gameObject.SetActive(false);
            }
            
            // Level 1 hint
            // if (GameManager.Instance.level1HintText.gameObject.activeSelf)
            // {
            //     GameManager.Instance.level1HintDisplayed = true;
            //     GameManager.Instance.level1HintText.gameObject.SetActive(false);
            //     Debug.Log("level 1 hint");
            //     Debug.Log(GameManager.Instance.level1HintDisplayed);
            // }
            
            // Level 2 hint
            // if (GameManager.Instance.level2HintText.gameObject.activeSelf)
            // {
            //     GameManager.Instance.level2HintDisplayed = true;
            //     GameManager.Instance.level2HintText.gameObject.SetActive(false);
            //     Debug.Log("level 2 hint");
            // }
        }
    }

    private void OnMouseDrag()
    {
        // Move the plane based on mouse pos
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        newPlanePos = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = newPlanePos;
    }

    private void OnMouseUp()
    {
        // Calculate the force on mouse release
        endTime = Time.time;
        endMousePos = Input.mousePosition;
        swipeDistance = (endMousePos - startMousePos).magnitude;
        swipeTime = endTime - startTime;

        if (swipeTime is >= 0.5f and < 5f && swipeDistance > 30f)
        {
            flying = true;
            CalculateAngle();
            CalculateSpeed();
            rb.isKinematic = false;
            rb.AddForce(new Vector3(angle.x * planeSpd, angle.y * planeSpd * 0.7f, angle.z * planeSpd));
            // Invoke("ResetPlane", 4f);
            
        }
        else
        {
            ResetPlane();
        }
    }

    void CalculateAngle()
    {
        angle = new Vector3(newPlanePos.x, newPlanePos.y * (-1), newPlanePos.z * (-1));
    }
    
    void CalculateSpeed()
    {
        if (swipeTime > 0)
        {
            planeSpd = swipeDistance * swipeTime * 0.6f;
        }

        if (planeSpd > maxPlaneSpd)
        {
            planeSpd = maxPlaneSpd;
        }
        swipeTime = 0;
    }
}
