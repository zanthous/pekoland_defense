using UnityEngine;
using System.Collections;
using System;

public enum CameraZoomLevel
{
	normal,
	far,
	farther,
	farthest
}

public class CameraFollowNew : MonoBehaviour
{
	public PlayerMovement target;
	public float verticalOffset;
	public float lookAheadDstX;
	public float lookSmoothTimeX;
	public float verticalSmoothTime;
	public Vector2 focusAreaSize;

	FocusArea focusArea;

	float currentLookAheadX;
	float targetLookAheadX;
	float lookAheadDirX;
	float smoothLookVelocityX;
	float smoothVelocityY;

	bool lookAheadStopped;

	private BoxCollider2D targetCollider;
	public static Action<CameraZoomLevel> ChangeCameraZoom;
	[SerializeField] private float zoomChangeDuration = 3.0f;
	private bool cameraZoomTransition = false;
	private new Camera camera;

	void Start()
	{
		targetCollider = target.GetComponent<BoxCollider2D>();
		focusArea = new FocusArea(targetCollider.bounds, focusAreaSize);
		ChangeCameraZoom += OnChangeCameraZoom;
		camera = GetComponent<Camera>();
	}

	void LateUpdate()
	{
		focusArea.Update(targetCollider.bounds);

		Vector2 focusPosition = focusArea.centre + Vector2.up * verticalOffset;

		if(focusArea.velocity.x != 0)
		{
			lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
			if(Mathf.Sign(target.Velocity.x) == Mathf.Sign(focusArea.velocity.x) && target.Velocity.x != 0)
			{
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDstX;
			}
			else
			{
				if(!lookAheadStopped)
				{
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
				}
			}
		}


		currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);

		focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
		focusPosition += Vector2.right * currentLookAheadX;
		transform.position = (Vector3) focusPosition + Vector3.forward * -10;
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

		while((greater && (camera.orthographicSize < newZoomLevel)) || (!greater && (camera.orthographicSize > newZoomLevel)))
		{
			timer += Time.deltaTime;

			camera.orthographicSize = Mathf.Lerp(initialSize, (float) newZoomLevel, (timer / zoomChangeDuration));

			yield return null;
		}

		cameraZoomTransition = false;
	}


	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1, 0, 0, .5f);
		Gizmos.DrawCube(focusArea.centre, focusAreaSize);
	}

	struct FocusArea
	{
		public Vector2 centre;
		public Vector2 velocity;
		float left, right;
		float top, bottom;


		public FocusArea(Bounds targetBounds, Vector2 size)
		{
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			velocity = Vector2.zero;
			centre = new Vector2((left + right) / 2, (top + bottom) / 2);
		}

		public void Update(Bounds targetBounds)
		{
			float shiftX = 0;
			if(targetBounds.min.x < left)
			{
				shiftX = targetBounds.min.x - left;
			}
			else if(targetBounds.max.x > right)
			{
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if(targetBounds.min.y < bottom)
			{
				shiftY = targetBounds.min.y - bottom;
			}
			else if(targetBounds.max.y > top)
			{
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			centre = new Vector2((left + right) / 2, (top + bottom) / 2);
			velocity = new Vector2(shiftX, shiftY);
		}
	}

}