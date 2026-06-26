using UnityEngine;

public class TwoDRotationExample : MonoBehaviour
{
	public float angle;
	void Start()
	{

	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Space))
		{
			Vector3 rotation = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle/*/ 1.0f*/), Mathf.Sin(Mathf.Deg2Rad * angle/* / 1.0f*/));
			transform.position = new Vector3(/*x: */transform.position.x * rotation.x - transform.position.y * rotation.y,// x
				/*y: */transform.position.x * rotation.y + transform.position.y * rotation.x,// y i
				/*z: */0.0f);
		}
	}
}