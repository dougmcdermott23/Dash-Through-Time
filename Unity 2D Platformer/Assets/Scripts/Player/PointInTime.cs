using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointInTime
{
	public Vector3 position;
	public int spriteIndex;
	public int facingRight;

	public PointInTime(Vector3 _position, int _spriteIndex, int _facingRight)
	{
		position = _position;
		spriteIndex = _spriteIndex;
		facingRight = _facingRight;
	}
}
