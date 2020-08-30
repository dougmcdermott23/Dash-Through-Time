using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public LayerMask collisionMask;

	[HideInInspector]
	public BoxCollider2D boxCollider2D;
	[HideInInspector]
	public RaycastOrigins raycastOrigins;

	[HideInInspector]
	public const float skinWidth = .015f;
	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	// Needs to be awake because it is called before start, eliminates errors with Camera Controller
	public virtual void Awake() {
		boxCollider2D = GetComponent<BoxCollider2D> ();

		if (boxCollider2D == null)
			Debug.Log ("BoxCollider2D missing");
	}

	public virtual void Start () {
		CalculateRaySpacing ();
	}

	public void UpdateRayCastOrigins() {
		Bounds bounds = boxCollider2D.bounds;
		bounds.Expand (skinWidth * -2);

		raycastOrigins.botLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.botRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	public void CalculateRaySpacing() {
		Bounds bounds = boxCollider2D.bounds;
		bounds.Expand (skinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 botLeft, botRight;
	}
}