using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;

public class TrinaxCore : MonoBehaviour
{
    private static TrinaxCore instance;

    public static TrinaxCore Instance {
        get {
            if (instance == null) {
                GameObject trinaxCore = new GameObject("_TriCore");
                instance = trinaxCore.AddComponent<TrinaxCore>();
                return instance;
            }
            else {
                return instance;
            }
        }

        set {
            instance = value;
        }
    }

	//public void LoadImageFromUrl(Image target, string url, System.Action callback = null, GameObject placeholder = null) {
 //       StartCoroutine(LoadImage(target, url, callback, placeholder));
 //   }

 //   IEnumerator LoadImage(Image target, string url, System.Action callback = null, GameObject placeholder = null) {
	//	WWW www = new WWW(url);
	//	yield return www;
 //       target.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
 //       if (callback != null) {
 //           callback();
 //       }
 //   }

    public async Task LoadImageFromUrlAsync(Image target, string url, bool useImgSizeFromUrl, System.Action<byte[]> callback = null, Image placeHolder = null)
    {
        if (placeHolder != null)
        {
            Debug.Log("Displaying placeholder first...");
            target.sprite = placeHolder.sprite;
            target.rectTransform.sizeDelta = new Vector2(placeHolder.mainTexture.width, placeHolder.mainTexture.height);
        }

        Debug.Log("Loading image from: " + url);
        var request = await new WWW(url);
        target.sprite = Sprite.Create(request.texture, new Rect(0, 0, request.texture.width, request.texture.height), new Vector2(0, 0));

        if (useImgSizeFromUrl)
        {
            target.rectTransform.sizeDelta = new Vector2(request.texture.width, request.texture.height);
        }

        callback(request.bytes);
    }

    public async Task LoadRawImageFromUrlAsync(RawImage target, string url, bool useImgSizeFromUrl, System.Action<byte[]> callback = null, RawImage placeHolder = null)
    {
        if(placeHolder != null)
        {
            Debug.Log("Displaying placeholder first...");
            target.texture = placeHolder.texture;
            target.rectTransform.sizeDelta = new Vector2(placeHolder.texture.width, placeHolder.texture.height);
        }

        Debug.Log("Loading image from: " + url);
        var request = await new WWW(url);

        Texture2D texture = new Texture2D(request.texture.width, request.texture.height, TextureFormat.ARGB32, false);
        texture.SetPixels(request.texture.GetPixels(0, 0, request.texture.width, request.texture.height));
        texture.Apply();

        if(useImgSizeFromUrl)
        {
            target.rectTransform.sizeDelta = new Vector2(request.texture.width, request.texture.height);
        }
        target.texture = texture;

        callback(request.bytes);
    }

    public void LoadImageFromBytes(Image target, byte[] imgData, bool useLoadedImgSize)
    {
        Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        texture.LoadImage(imgData);
        texture.Apply();

        target.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        if(useLoadedImgSize)
            target.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
    }

    public void LoadRawImageFromBytes(RawImage target, byte[] imgData, bool useLoadedImgSize)
    {
        Texture2D texture = new Texture2D(0, 0, TextureFormat.ARGB32, false);
        texture.LoadImage(imgData);
        texture.Apply();

        target.texture = texture;
        if(useLoadedImgSize)
            target.rectTransform.sizeDelta = new Vector2(texture.width, texture.height);
    }

    public void SendEmailInBackground(string sender, string senderPassword, string receiver, string title, string body, string smtpServerName = "smtp.gmail.com") {
        MailMessage mail = new MailMessage {
            From = new MailAddress(sender),
            Subject = title,
            Body = body,
        };
        mail.To.Add(receiver);

        Debug.Log("Connecting to SMTP server");
        SmtpClient smtpServer = new SmtpClient(smtpServerName) {
            Port = 587,
            Credentials = new NetworkCredential(sender, senderPassword) as ICredentialsByHost,
            EnableSsl = true
        };
        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        };
        Debug.Log("Sending message");

        smtpServer.SendCompleted += (s, e) => {
            mail.Dispose();
            Debug.Log("Send Completed");
        };

        try {
            smtpServer.SendAsync(mail, null);
        } catch (SmtpException e) {
            Debug.Log(e);
        }
    }
}
