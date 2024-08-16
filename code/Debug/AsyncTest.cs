
public class AsyncTest : Component
{
	[Button("Start Async")]
	public void StartAsync()
	{
		ConstantLogging();
	}

	async void ConstantLogging()
	{
		while (true)
		{
			Log.Info("ConstantLogging()");
			await Task.Frame();
			//await GameTask.Delay(1);

			if (GameObject == null)
			{
				Log.Info($"ConstantLogging() GameObject is null!");
				break;
				//return;
			}
		}

		Log.Info($"ConstantLogging() End!");
	}
}
