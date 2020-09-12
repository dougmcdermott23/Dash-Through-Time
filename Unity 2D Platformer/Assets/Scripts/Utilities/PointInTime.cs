using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointInTime
{
	// In the future, sprite animations will also be stored here so they will appear in the rewind animation

	public Vector3 position;

	public PointInTime(Vector3 _position)
	{
		position = _position;
	}
}
