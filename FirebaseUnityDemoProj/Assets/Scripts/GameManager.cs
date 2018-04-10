using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameController MyGameController;
    public GameObject SigninCanvas;
    public GameObject GameCanvas;
    public GameObject BlockUserCanvas;

    public Button SigninButton;
	
	void Start () {
        Debug.Log("Hello app :-)");

        GameCanvas.SetActive(false);

        BlockUserCanvas.SetActive(false);

        EventManagerScript.Instance.StartListening(EventManagerScript.EVENT__BLOCK_USER, OnUserBlocked);

        // Initialize firebase
        FirebaseUtils.Instance.Initialize();
	}
	
    public void OnUISignin()
    {
        // Disable the signin button while signing in
        SigninButton.interactable = false;

        EventManagerScript.Instance.StartListening(EventManagerScript.EVENT__ANONYMOUS_LOGIN_RESULT, OnLoginResult);

        // Make the sign in
        FirebaseUtils.Instance.SignInAnonumous();
    }

    private void OnLoginResult(object obj)
    {
        EventManagerScript.Instance.StopListening(EventManagerScript.EVENT__ANONYMOUS_LOGIN_RESULT, OnLoginResult);

        if (obj == null)
        {
            // Everything is ok
            GameCanvas.SetActive(true);
            SigninCanvas.SetActive(false);

            MyGameController.StartGame();
        }
        else
        {
            // Enable back the signin button 
            SigninButton.interactable = true;

            Debug.Log("Signin did not succeed");
        }
    }

    private void OnUserBlocked(object obj)
    {
        // Show the blocked user canvas
        BlockUserCanvas.SetActive(true);
    }
        
}
