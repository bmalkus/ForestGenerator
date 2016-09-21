using UnityEngine;

public class MovingCamera : MonoBehaviour {

  public float speed = 10f;
  public float sensitivity = 0.8f;
  public float rotationSpeed = 20f;

  public float forceAdd = 10f;

  public Vector3 dir;

  // Use this for initialization
  void Start()
  {
    dir = new Vector3(1f, 0f, 1f);
    GetComponent<Camera>().cullingMask = 0x07;
  }

  void Update()
  {
    transform.LookAt(transform.position + dir);
  }

  void FixedUpdate()
  {

    if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.F))
      dir = Quaternion.AngleAxis(rotationSpeed, Vector3.up) * dir;

    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.S))
      dir = Quaternion.AngleAxis(-rotationSpeed, Vector3.up) * dir;

    if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.D))
      transform.Translate(new Vector3(0, 0, -speed * Time.deltaTime));

    if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.E))
      transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));

    if (Input.GetKey(KeyCode.W))
      transform.Translate(new Vector3(-speed * Time.deltaTime, 0, 0));

    if (Input.GetKey(KeyCode.R))
      transform.Translate(new Vector3(speed * Time.deltaTime, 0, 0));

    if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.RightBracket))
      if (speed < 100f)
        speed += 1f;

    if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.LeftBracket))
      if (speed > 10f)
        speed -= 1f;

    if (Input.GetMouseButton(0))
    {
      dir = Quaternion.AngleAxis(sensitivity * - Input.GetAxis("Mouse Y"), transform.right) * dir;
      dir = Quaternion.AngleAxis(sensitivity * Input.GetAxis("Mouse X"), Vector3.up) * dir;
      dir.Normalize();
    }

    if (Input.GetKey(KeyCode.Space))
      GetComponent<Rigidbody>().AddForce(new Vector3(0, forceAdd, 0), ForceMode.Impulse);
  }

}
