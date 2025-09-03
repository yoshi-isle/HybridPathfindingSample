using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;
    public float distanceFromPlayer = 10f;
    public float maxTopAngle = 80f;
    public float maxBottomAngle = 20f;
    public float mouseSensitivity = 5f;
    public float wasdSensitivity = 1f;
    public float smoothTime = 0.3f;
    public float zoomSpeed = 2f;
    public float minDistance = 5f;
    public float maxDistance = 20f;

    private float currentX = 0f;
    private float currentY = 0f;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        currentX = transform.eulerAngles.y;
        currentY = transform.eulerAngles.x;
    }

    private void Update()
    {
        if (Input.GetMouseButton(2))
        {
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, -maxBottomAngle, maxTopAngle);
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        currentX += horizontal * wasdSensitivity * Time.deltaTime * 50f;
        currentY -= vertical * wasdSensitivity * Time.deltaTime * 50f;
        currentY = Mathf.Clamp(currentY, -maxBottomAngle, maxTopAngle);

        distanceFromPlayer -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distanceFromPlayer = Mathf.Clamp(distanceFromPlayer, minDistance, maxDistance);
    }

    private void LateUpdate()
    {
        Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = playerTransform.position + rotation * direction;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(playerTransform);
    }
}