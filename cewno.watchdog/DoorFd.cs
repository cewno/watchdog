namespace cewno.watchdog;

/// <summary>
/// 门句柄句柄api
/// </summary>
public abstract class WatchFdAbstract
{
	/// <summary>
	/// 超时时运行的方法
	/// </summary>
	public abstract void Rm();
	/// <summary>
	/// 超时时间戳
	/// </summary>
	public long time;
}
/// <summary>
/// 默认门句柄句柄实现
/// </summary>
public class WatchFd : WatchFdAbstract
{
	/// <summary>
	/// 创建门句柄
	/// </summary>
	/// <param name="outTimeLength">超时时间</param>
	/// <param name="rmAction">超时时运行的任务</param>
	public WatchFd(uint outTimeLength, Action rmAction)
	{
		_outTimeLength = outTimeLength;
		time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + outTimeLength;
		_rmAction = rmAction;
	}

	/// <summary>
	/// 超时时间
	/// </summary>
	private long _outTimeLength;
	/// <summary>
	/// 超时时运行的任务
	/// </summary>
	private readonly Action _rmAction;
	
	/// <summary>
	/// 重置时间（喂狗）
	/// </summary>
	public void Reset()
	{
		time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _outTimeLength;
	}
	/// <summary>
	/// 重置时间（喂狗）
	/// </summary>
	public void Reset(int outTimeLength)
	{
		time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + outTimeLength;
		_outTimeLength = outTimeLength;
	}
	/// <summary>
	/// 超时时运行的方法
	/// </summary>
	public override void Rm()
	{
		_rmAction.Invoke();
	}
	
}