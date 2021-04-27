using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	enum CameraState
    {
		normal,
		cutscene
    }

	[SerializeField] private Transform target;
	[SerializeField] private float smoothSpeed = 0.08f;

	//wish I had a better name for this one
	[Tooltip("example: 0.3 would represent 30% of the left half of the screen")]
	[SerializeField] private float goLeftWithoutMovingCameraAmount = 0.2f;
	[Tooltip("default number of units above player's center the camera should follow from")]
	[SerializeField] private float defaultCameraHeight = 1.8f;

	private float width;
	private float height;

	private void Start()
    {
		Camera cam = Camera.main;
		height = 2f * cam.orthographicSize;
		width = height * cam.aspect;
	}

    void FixedUpdate()
	{
		Vector2 desiredPosition = transform.position;
		var leftThresholdPoint = transform.position.x - ((width / 2.0f) * goLeftWithoutMovingCameraAmount);
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
		if(target.transform.position.y > transform.position.y)
        {
			desiredPosition.y = target.transform.position.y;
        }
		if(target.transform.position.y < transform.position.y - defaultCameraHeight)
        {
			desiredPosition.y = transform.position.y - defaultCameraHeight;
		}
		Vector2 smoothedPosition = Vector2.Lerp(transform.position, desiredPosition, smoothSpeed);
		transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y,-10);
	}

}