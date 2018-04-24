using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class FirebaseUtils : Singleton<FirebaseUtils> {

    
    private DatabaseReference mDatabaseRef;

    //private bool mTokenNeedToBeSentToServer = false;
    //private string mDeviceIdToken;
    

    private Firebase.Auth.FirebaseAuth mAuth;
    private Firebase.Auth.FirebaseUser mUser;

    protected FirebaseUtils()
    {
    }

    public void Initialize()
    {
        mAuth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        if (mAuth != null)
        {
            mUser = mAuth.CurrentUser;
        }

        
        // Set this before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(Constants.FIREBASE_DATABASE_URL);

        mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;


        Firebase.RemoteConfig.FirebaseRemoteConfig.FetchAsync(System.TimeSpan.Zero).ContinueWith(FetchComplete);        
    }

    public void SignInAnonumous()
    {
        if (mAuth.CurrentUser == null)
        {
            mAuth.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");

                    mUser = null;

                    EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__ANONYMOUS_LOGIN_RESULT, "canceled");
                }
                else if (task.IsFaulted)
                {
                    mUser = null;

                    EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__ANONYMOUS_LOGIN_RESULT, ("error: " + task.Exception));
                }
                else
                {
                    mUser = task.Result;

                    Debug.LogFormat("User signed in successfully: {0} ({1})", mUser.DisplayName, mUser.UserId);

                    EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__ANONYMOUS_LOGIN_RESULT, null);
                }
            });
        }
        else
        {
            Debug.Log("We are already signed in!");

            EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__ANONYMOUS_LOGIN_RESULT, null);
        }
    }

    public void SetUserLocation(int x, int y)
    {
        if (mUser != null)
        {
            string userGameStatusJSONStr = "{\"x\" : \"" + x.ToString() + "\"";
            userGameStatusJSONStr += " \"y\":\"" + y.ToString() + "\"}";

            Debug.Log("Setting user value: " + userGameStatusJSONStr);

            mDatabaseRef.Child("users").Child(mUser.UserId).SetRawJsonValueAsync(userGameStatusJSONStr);
        }
    }

    public void StartGettingUsersLocations()
    {
        FirebaseDatabase.DefaultInstance.GetReference("users").ChildChanged += OnUserChanged;
        FirebaseDatabase.DefaultInstance.GetReference("users").ChildAdded += OnUserChanged;
    }

    void OnUserChanged(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        
        // Verify that this is not us
        if (args.Snapshot.Key != mUser.UserId)
        {
            UserDataStruct userData = new UserDataStruct();
            userData.id = args.Snapshot.Key;

            string xStrValue = args.Snapshot.Child("x").Value as string;
            userData.x = int.Parse(xStrValue);

            string yStrValue = args.Snapshot.Child("y").Value as string;
            userData.y = int.Parse(yStrValue);

            EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__USER_LOCATION_UPDATED, userData);
        }
    }
    
    void FetchComplete(Task fetchTask)
    {
       if (fetchTask.IsCompleted)
        {
            if (Firebase.RemoteConfig.FirebaseRemoteConfig.Info.LastFetchStatus == Firebase.RemoteConfig.LastFetchStatus.Success)
            {
                Debug.Log("Fetch completed successfully on: " + Firebase.RemoteConfig.FirebaseRemoteConfig.Info.FetchTime.ToLocalTime());

                if (Firebase.RemoteConfig.FirebaseRemoteConfig.ActivateFetched())
                {
                    // "blockUser"
                    Firebase.RemoteConfig.ConfigValue cv = Firebase.RemoteConfig.FirebaseRemoteConfig.GetValue("blockUser");

                    // Verify that this value exists
                    if ((cv.Source != Firebase.RemoteConfig.ValueSource.StaticValue) && !string.IsNullOrEmpty(cv.StringValue))
                    {
                        bool blockUser = cv.BooleanValue;

                        if (blockUser)
                        {
                            EventManagerScript.Instance.TriggerEvent(EventManagerScript.EVENT__BLOCK_USER, null);
                        }
                    }      
                }
            }

        }
    }
}