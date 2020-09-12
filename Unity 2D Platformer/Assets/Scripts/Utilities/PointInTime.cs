using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointInTime
{
	// In the future, sprite animations will also be stored here so they will appear in the rewind animation

	public Vector3 position;
	public Quaternion rotation;
	public Vector3 scale;

	public PointInTime(Vector3 _position, Quaternion _rotation, Vector3 _scale)
	{
		position = _position;
		rotation = _rotation;
		scale = _scale;
	}
}
