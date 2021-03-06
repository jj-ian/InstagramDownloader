using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DownloadIGMedia : MonoBehaviour
{

	public string username;
	public string hashtag;

	// downloads all Instagram media for a user
	public void DownloadMedia ()
	{
		//IEnumerator coroutine = DownloadMediaForUsername (username);
		IEnumerator coroutine = DownloadMediaForHashtag("dogs");

		StartCoroutine (coroutine);
	}

	private IEnumerator DownloadMediaForHashtag (string hashtag)
	{
		Debug.Log ("Downloading Instagram media for hashtag " + hashtag);

		// could probably do it faster by pulling each page as a coroutine
		topLevelDataHashtag topLevelDataHashtag;
		string queryOptions = "";
		int count = 1;
		do {
			Debug.Log("count " + count);
			// build API request
			//string url = "https://www.instagram.com/" + username + "/media/" + queryOptions;
			string url = "https://www.instagram.com/explore/tags/adobepremiereprocs5/?__a=1" + queryOptions;

			// grab the response JSON
			WWW www = new WWW (url);
			yield return www;
			string json = www.text;
		Debug.Log (json);
		topLevelDataHashtag = JsonUtility.FromJson<topLevelDataHashtag> (json);
			Debug.Log ("has next page: " + topLevelDataHashtag.tag.media.page_info.has_next_page);
		Debug.Log ("end cursor: " + topLevelDataHashtag.tag.media.page_info.end_cursor);

			// download each image in the response. 
			// Currently getting the low-res 320x320 preview for each image and video. 
			// To pull higher-res images or actual video files, change the arguments below.
		foreach (node node in topLevelDataHashtag.tag.media.nodes) {
				string id = node.id;
				Debug.Log ("img id: " + id);
			string thumbnailSrc = node.thumbnail_src;

			IEnumerator imgDownloadCoroutine = DownloadImageFromURL (thumbnailSrc);
				StartCoroutine (imgDownloadCoroutine);
			}
			
			string endCursor = topLevelDataHashtag.tag.media.page_info.end_cursor;
			queryOptions = "&max_id=" + endCursor;

			// if there are more images, pull the next page
			count++;
		} while (!topLevelDataHashtag.tag.media.page_info.end_cursor.Equals("null"));
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

		File.WriteAllBytes ("Media/" + fileName, imgBytes);
	}
}

// for JSON parsing
// username
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

//hashtag

[System.Serializable]

public struct topLevelDataHashtag
{
	public tag tag;
}

[System.Serializable]

public class tag
{
	public media media;
}

[System.Serializable]
public class media
{
	public List<node> nodes;
	public page_info page_info;

}

[System.Serializable]
public class node
{
	public string id;
	public string thumbnail_src;

}

[System.Serializable]
public class page_info
{
	public bool has_next_page;
	public string end_cursor;

}

