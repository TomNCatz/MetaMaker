using System.Threading.Tasks;

namespace LibT;

public class AsyncTrigger
{
	private bool _triggered;
	
	public AsyncTrigger()
	{
		_triggered = false;
	}

	public async Task AwaitThis()
	{
		while (!_triggered)
		{
			await Task.Delay(300);
		}
	}

	public void Trigger()
	{
		_triggered = true;
	}
}