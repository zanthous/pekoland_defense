using System;
using System.Collections;
using UnityEngine;

public enum CameraZoomLevel
{
	normal,
	far,
	farther,
	farthest
}

public class CameraFollow : MonoBehaviour
{
	enum CameraState
    {
		normal,
		cutscene
    }

	public static Action<CameraZoomLevel> ChangeCameraZoom;

	[SerializeField] private Transform target;
	[SerializeField] private float smoothSpeed = 0.08f;

	//wish I had a better name for this one
	[Tooltip("How far can the player backtrack leftward without moving the camera?")]
	[SerializeField] private float leftBuffer = 3.0f;
	[Tooltip("How far can the player move upward without moving the camera relative to the camera? - some is necessary so jump doesn't move it")]
	[SerializeField] private float upBuffer = 2.0f;
	[Tooltip("default number of units above player's center the camera should follow from")]
	[SerializeField] private float defaultCameraHeight = 1.8f;

	[SerializeField] private float zoomChangeDuration = 3.0f;
	
	[SerializeField] private bool showDebug = false;

	private float width;
	private float height;

	private Camera camera;

	private bool cameraZoomTransition = false;
		
	private void Start()
    {
		camera = Camera.main;
		height = 2f * camera.orthographicSize;
		width = height * camera.aspect;

		ChangeCameraZoom += OnChangeCameraZoom;
	}

    void FixedUpdate()
	{
		Vector2 desiredPosition = transform.position;
		var leftThresholdPoint = transform.position.x - leftBuffer;
		//Smooth follow on the x axis if past center of screen, or a specified amount to the left
		if(target.transform.position.x > transform.position.x)
		{
			desiredPosition.x = target.transform.position.x;
		}
		else if(target.transform.position.x < leftThresholdPoint)
		{
			//desired position is current camera position - (the distance player is past left threshold)
			desiredPosition.x = transform.position.x - (leftThresholdPoint - target.transform.position.x);
		}
		//Smooth follow on the y axis if moved up more than a jump's height approximately, and down if below initial height
		if(target.transform.position.y > transform.position.y + upBuffer)
        {
			desiredPosition.y = target.transform.position.y + upBuffer;
        }
		if(target.transform.position.y < transform.position.y - defaultCameraHeight)
        {
			desiredPosition.y = transform.position.y - defaultCameraHeight;
		}
		Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y,-10);
	}

	void OnChangeCameraZoom(CameraZoomLevel zoomLevel)
	{
		if(cameraZoomTransition)
			return;
		switch(zoomLevel)
		{
			case CameraZoomLevel.normal:
				StartCoroutine(SmoothCameraZoomChange(5));
				break;
			case CameraZoomLevel.far:
				StartCoroutine(SmoothCameraZoomChange(6));
				break;
			case CameraZoomLevel.farther:
				StartCoroutine(SmoothCameraZoomChange(7));
				break;
			case CameraZoomLevel.farthest:
				StartCoroutine(SmoothCameraZoomChange(8));
				break;
		}
	}

	private IEnumerator SmoothCameraZoomChange(int newZoomLevel)
    {
		cameraZoomTransition = true;

		float timer = 0.0f;
		bool greater = newZoomLevel > camera.orthographicSize;
		float initialSize = camera.orthographicSize;

		while( (greater && (camera.orthographicSize < newZoomLevel))  || (!greater && (camera.orthographicSize > newZoomLevel)) )
		{
			timer += Time.deltaTime;
		
			camera.orthographicSize = Mathf.Lerp(initialSize, (float) newZoomLevel, (timer / zoomChangeDuration));
			 
			yield return null;
		}

		cameraZoomTransition = false;
	}

    //Debug tool to show borders where the player can move without having the camera adjust
    private void OnDrawGizmosSelected()
    {

        if(showDebug)
        {
            Camera cam = Camera.main;
            height = 2f * cam.orthographicSize;
            width = height * cam.aspect;

            var leftThresholdPoint = transform.position.x - leftBuffer;

            Gizmos.DrawLine(new Vector3(leftThresholdPoint, -10, 0), new Vector3(leftThresholdPoint, 10, 0));
            Gizmos.DrawLine(new Vector3(transform.position.x, -10, 0), new Vector3(transform.position.x, 10, 0));

            Gizmos.DrawLine(new Vector3(-20 + transform.position.x, transform.position.y + upBuffer, 0), 
				new Vector3(20 + transform.position.x, transform.position.y + upBuffer, 0));
            Gizmos.DrawLine(new Vector3(-20 + transform.position.x, transform.position.y - defaultCameraHeight, 0), 
				new Vector3(20 + transform.position.x, transform.position.y - defaultCameraHeight, 0));
        }
    }
}