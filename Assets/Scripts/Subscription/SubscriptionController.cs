
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Core;
using UnityEngine;
using UnityEngine.Networking;

namespace Subscription
{
	public class SubscriptionController
	{
		public static bool IsSubscriptionActive = false;

		public const string URL_SUBSCRIPTION =
            "https://api.comfort-health.net/api/installations/subscription?token=30b1ebfd3225b7b0454854ad59135df86d78372d70bb0a553d1e417c3f7bb3df"; //https://ed99-37-73-146-24.eu.ngrok.io | https://8eba-37-73-91-76.eu.ngrok.io

        private const string HEADER_KEY = "InstallationId";
		private const string SUBSCRIPTION_ACTIVE = "live";
		private const string SUBSCRIPTION_TRIAL = "trial";
		private const string SUBSCRIPTION_INACTIVE_MESSAGE = "Issues with your subscription. Check your account.";
	    private const int QTS_CHECK_DELAY = 60000;//4000;

		public SubscriptionController()
		{
			SubscriptionHandler();
		}

		private async void SubscriptionHandler()
		{
			//CoroutineRunner.Instance.LaunchCoroutine(GetSubscription());
		    await GetSubscriptionTask();
			await Task.Delay(QTS_CHECK_DELAY);

			SubscriptionHandler();
		}

	    public async Task GetSubscriptionTask()
	    {
            using var www = UnityWebRequest.Get(URL_SUBSCRIPTION);
            www.SetRequestHeader(HEADER_KEY, SystemInfo.deviceUniqueIdentifier); //this is test. in prod use SystemInfo.deviceUniqueIdentifier as value instead of AndrewTest
            await www.SendWebRequest();

            try
            {
	            var data = JsonUtility.FromJson<SubscriptionData>(www.downloadHandler.text);
	            Debug.Log(data.subscription_status);
	            var isActive = data.subscription_status is SUBSCRIPTION_ACTIVE or SUBSCRIPTION_TRIAL;

	            IsSubscriptionActive = isActive;
	            
	            if (isActive)
		            InputBlocker.Unblock();
	            else
		            InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);
            }
            catch
            {
	            InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);
	            Debug.LogError("Id is not exist or another problem");
            }
            
            www.Dispose();
        }

		public IEnumerator GetSubscription()
		{
			using var www = UnityWebRequest.Get(URL_SUBSCRIPTION);
			www.SetRequestHeader(HEADER_KEY, SystemInfo.deviceUniqueIdentifier); //this is test. in prod use SystemInfo.deviceUniqueIdentifier as value instead of AndrewTest

			yield return www.SendWebRequest();


            var data = JsonUtility.FromJson<SubscriptionData>(www.downloadHandler.text);
            Debug.Log(data.subscription_status);
            var isActive = data.subscription_status is SUBSCRIPTION_ACTIVE or SUBSCRIPTION_TRIAL;

            IsSubscriptionActive = isActive;

            if (isActive)
                InputBlocker.Unblock();
            else
                InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);

//            try
//			{
//				var data = JsonUtility.FromJson<SubscriptionData>(www.downloadHandler.text);
//				Debug.Log(data.subscription_status);
//				var isActive = data.subscription_status is SUBSCRIPTION_ACTIVE or SUBSCRIPTION_TRIAL;
//
//				IsSubscriptionActive = isActive;
//
//				if (isActive)
//					InputBlocker.Unblock();
//				else
//					InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);
//			}
//			catch (Exception e)
//			{
//				Debug.Log("Failed to receive the subscription. Error: " + e.Message);
//
//				InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);
//			}
			
			www.Dispose();
		}
	}
}

public class UnityWebRequestAwaiter : INotifyCompletion
{
    private UnityWebRequestAsyncOperation asyncOp;
    private Action continuation;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
    {
        this.asyncOp = asyncOp;
        asyncOp.completed += OnRequestCompleted;
    }

    public bool IsCompleted { get { return asyncOp.isDone; } }

    public void GetResult() { }

    public void OnCompleted(Action continuation)
    {
        this.continuation = continuation;
    }

    private void OnRequestCompleted(AsyncOperation obj)
    {
        continuation();
    }
}

public static class ExtensionMethods
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}

/*
// Usage example:
UnityWebRequest www = new UnityWebRequest();
// ...
await www.SendWebRequest();
Debug.Log(req.downloadHandler.text);
*/
