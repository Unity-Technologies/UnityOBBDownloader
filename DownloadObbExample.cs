using UnityEngine;
using System.Collections;

public class DownloadObbExample : MonoBehaviour {
	private string expPath;
	private string logtxt;
	private bool alreadyLogged;
	public string nextScene;
	private bool downloadStarted;

	public Texture2D background;
	public ScaleMode scaleMode = ScaleMode.ScaleAndCrop;

	private WWW www;

	void Awake() {
#if !UNITY_ANDROID
		Debug.LogError("THIS LOADER SCENE SHOULD NEVER BE CONFIGURED ON NON ANDROID PLATFORMS");
#endif
	}

	void Start() {
		alreadyLogged = false;
	}

#if UNITY_ANDROID && !UNITY_EDITOR
	void log( string t )
	{
		logtxt += t + "\n";
		print("MYLOG " + t);
	}

	void OnGUI()
	{
		if (background != null)
			GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), background, scaleMode);

		if (!GooglePlayDownloader.RunningOnAndroid())
		{
			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "Use GooglePlayDownloader only on Android device!");
			return;
		}

		expPath = GooglePlayDownloader.GetExpansionFilePath();
		if (expPath == null)
		{
			GUI.Label(new Rect(10, 10, Screen.width-10, 20), "External storage is not available!");
		}
		else
		{
			string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
			string patchPath = GooglePlayDownloader.GetPatchOBBPath(expPath);
			if( alreadyLogged == false )
			{
				alreadyLogged = true;
				log( "expPath = "  + expPath );
				log( "Main = "  + mainPath );

				if (mainPath != null) {
					log( "Main = " + mainPath.Substring(expPath.Length));
					StartCoroutine(loadLevel());
				}

			}
			//GUI.Label(new Rect(10, 10, Screen.width-10, Screen.height-10), logtxt );

			if (mainPath == null)
			{
				GUI.Label(new Rect(Screen.width-600, Screen.height-230, 430, 60), "The game needs to download 100MB of game content. It's recommanded to use WIFI connexion.");
				if (GUI.Button(new Rect(Screen.width-500, Screen.height-170, 250, 60), "Start Download !"))
				{
					GooglePlayDownloader.FetchOBB();
					StartCoroutine(loadLevel());
				}
			} else {
				if (www != null && !www.isDone) {
					float percent = ((int)(www.progress * 1000))/10.0f;
					GUI.Label(new Rect(Screen.width-500, Screen.height-170, 430, 60),
						"Expanding... " + percent + "%");
				}
			}
		}

	}
	protected IEnumerator loadLevel()
	{ 
		string mainPath;
		do
		{
			yield return new WaitForSeconds(0.5f);
			mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
			log("waiting mainPath "+mainPath);
		}
		while( mainPath == null);

		if( downloadStarted == false )
		{
			downloadStarted = true;

			string uri = "file://" + mainPath;
			log("downloading " + uri);
			www = WWW.LoadFromCacheOrDownload(uri , 0);

			// Wait for download to complete
			yield return www;

			if (www.error != null)
			{
				log ("wwww error " + www.error);
			}
			else
			{
				Application.LoadLevel(nextScene);
			}
		}
	}
#endif
}
