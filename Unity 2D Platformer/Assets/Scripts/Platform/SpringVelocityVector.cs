using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringVelocityVector
{
    public Vector3 SpringVelocity { get; private set; }
    public bool AssignSpringVelocity { get; private set; }

    public SpringVelocityVector(Vector3 _springVelocity, bool _assignSpringVelocity)
    {
        SpringVelocity = _springVelocity;
        AssignSpringVelocity = _assignSpringVelocity;
    }
}
