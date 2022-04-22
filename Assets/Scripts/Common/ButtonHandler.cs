using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{

    /// <summary>
    ///   React to a button click event.  Used in the UI Button action definition.
    /// </summary>
    /// <param name="button"></param>
    public void onButtonClicked(Button button)
    {
        // which GameObject?
        GameObject go = GameObject.Find("GameController");
        if (go != null)
        {
            TestHome gameController = go.GetComponent<TestHome>();
            if (gameController == null)
            {
                Debug.LogError("Missing game controller...");
                return;
            }
            switch (button.name)
            {
                case "JoinButton":
                    gameController.onJoinButtonClicked();
                    break;
                case "SendMsg":
                    DemoGameManager.Instance().SendTextMessage();
                    break;
            }
        }
    }
}
