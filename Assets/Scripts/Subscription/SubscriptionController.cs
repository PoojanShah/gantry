
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Subscription
{
	public class SubscriptionController
	{
		public const string URL_SUBSCRIPTION =
			"https://9838-37-73-81-3.eu.ngrok.io/api/installations/subscription?token=30b1ebfd3225b7b0454854ad59135df86d78372d70bb0a553d1e417c3f7bb3df";

		private const string HASH_ID = "localId";
		private const int QTS_CHECK_DELAY = 4000;
		private const int QTS_RESPONSE_DELAY = 10;

		private bool _isSubscriptionChecking = false;

		public SubscriptionController()
		{
			_isSubscriptionChecking = true;

			GetSubscription();
		}

		public void CleanUp() => _isSubscriptionChecking = false;

		public async void GetSubscription()
		{
			while (_isSubscriptionChecking)
			{
				var form = new WWWForm();
				form.AddField(HASH_ID, "632de9097e640");

				using var www = UnityWebRequest.Post(URL_SUBSCRIPTION, form);
				www.SendWebRequest();

				if (!www.isDone)
					await Task.Delay(QTS_RESPONSE_DELAY);

				try
				{
					var received = JsonUtility.FromJson<SubscriptionData>(www.downloadHandler.text);

					Debug.Log(received.subscription_status);
				}
				catch (Exception e)
				{
					Debug.Log("cant get subscription " + e.Message);
				}

				await Task.Delay(QTS_CHECK_DELAY);
			}
		}
	}
}
