using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DownloadIGMedia : MonoBehaviour
{

	public string username;

	// downloads all Instagram media for a user
	public void DownloadMedia ()
	{
		IEnumerator coroutine = DownloadMediaForUsername (username);
		StartCoroutine (coroutine);
	}

	private IEnumerator DownloadMediaForUsername (string username)
	{
		Debug.Log ("Downloading Instagram media for user " + username);

		// could probably do it faster by pulling each page as a coroutine
		topLevelData data;
		string queryOptions = "";

		do {
			// build API request
			string url = "https://www.instagram.com/" + username + "/media/" + queryOptions;

			// grab the response JSON
			WWW www = new WWW (url);
			yield return www;
			string json = www.text;
			data = JsonUtility.FromJson<topLevelData> (json);
			Debug.Log ("Response status: " + data.status);
			Debug.Log ("More data available: " + data.more_available);

			// download each image in the response. 
			// Currently getting the low-res 320x320 preview for each image and video. 
			// To pull higher-res images or actual video files, change the arguments below.
			foreach (items item in data.items) {
				string imgURL = item.images.low_resolution.url;
				Debug.Log ("Downloading image: " + imgURL);
				IEnumerator imgDownloadCoroutine = DownloadImageFromURL (imgURL);
				StartCoroutine (imgDownloadCoroutine);
			}

			string lastItemID = data.items [data.items.Count - 1].id;
			queryOptions = "?max_id=" + lastItemID;

			// if there are more images, pull the next page
		} while (data.more_available);
	}

	// save to file the image at imgURL
	private IEnumerator DownloadImageFromURL (string imgURL)
	{
		Debug.Log ("Downloading image: " + imgURL);
		WWW wwwImg = new WWW (imgURL);
		yield return wwwImg;

		byte[] imgBytes = wwwImg.bytes;

		string fileName = Path.GetFileName (imgURL);

		File.WriteAllBytes ("Assets/Media/" + fileName, imgBytes);
	}
}

// for JSON parsing
[System.Serializable]

public struct topLevelData
{
	public string status;
	public bool more_available;
	public List<items> items;
}

[System.Serializable]
public class items
{
	public string id;
	public images images;

}

[System.Serializable]
public class images
{
	public lowResolution low_resolution;

}

[System.Serializable]
public class lowResolution
{
	public int height;
	public int width;
	public string url;

}