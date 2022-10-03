
using System;
using System.Collections;
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
			"https://9838-37-73-81-3.eu.ngrok.io/api/installations/subscription?token=30b1ebfd3225b7b0454854ad59135df86d78372d70bb0a553d1e417c3f7bb3df";

		private const string HASH_ID = "localId";
		private const string SUBSCRIPTION_ACTIVE_MESSAGE = "active";
		private const string SUBSCRIPTION_INACTIVE_MESSAGE = "Issues with your subscription. Check your account.";
		private const int QTS_CHECK_DELAY = 4000;

		public SubscriptionController()
		{
			//SubscriptionHandler();
		}

		private async void SubscriptionHandler()
		{
			CoroutineRunner.Instance.LaunchCoroutine(GetSubscription());

			await Task.Delay(QTS_CHECK_DELAY);

			SubscriptionHandler();
		}

		public IEnumerator GetSubscription()
		{
			var form = new WWWForm();
			form.AddField(HASH_ID, "632de9097e640");

			using var www = UnityWebRequest.Post(URL_SUBSCRIPTION, form);

			yield return www.SendWebRequest();

			try
			{
				var data = JsonUtility.FromJson<SubscriptionData>(www.downloadHandler.text);

				var isActive = data.subscription_status == SUBSCRIPTION_ACTIVE_MESSAGE;

				IsSubscriptionActive = isActive;

				if (isActive)
					InputBlocker.Unblock();
				else
					InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);
			}
			catch (Exception e)
			{
				Debug.Log("Failed to receive the subscription. Error: " + e.Message);

				InputBlocker.Block(SUBSCRIPTION_INACTIVE_MESSAGE);
			}
			
			www.Dispose();
		}
	}
}
