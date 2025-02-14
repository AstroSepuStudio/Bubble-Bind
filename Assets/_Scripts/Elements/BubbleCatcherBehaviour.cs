using UnityEngine;

public class BubbleCatcherBehaviour : MonoBehaviour
{
    [SerializeField] BubbleBindElement.Element _catcherColor;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_catcherColor == BubbleBindElement.Element.RedCatcher)
        {
            if (collision.gameObject.CompareTag("RedBubble"))
            {
                collision.gameObject.GetComponent<BubbleBehaviour>().PopBubble();
            }
        }

        if (_catcherColor == BubbleBindElement.Element.BlueCatcher)
        {
            if (collision.gameObject.CompareTag("BlueBubble"))
            {
                collision.gameObject.GetComponent<BubbleBehaviour>().PopBubble();
            }
        }
    }
}
