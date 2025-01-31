using UnityEngine;

[RequireComponent(typeof(FixedJoint2D))]
public class BubblyStickyComponent : MonoBehaviour
{
    [SerializeField] FixedJoint2D _joint;

    bool _rbLinked;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("BlueBubble") ||
             collision.gameObject.CompareTag("RedBubble"))
        {
            _joint.enabled = true;
            _joint.connectedBody = collision.rigidbody;
        }
    }
}
