namespace cewno.watchdog;

/// <summary>
/// 基本看门狗 API
/// </summary>
public abstract class WatchDogAbstract
{
	/// <summary>
	/// 注册门句柄
	/// </summary>
	/// <param name="watchFd">门句柄</param>
	public abstract void Reg(WatchFdAbstract watchFd);
	
	/// <summary>
	/// 注销门句柄
	/// </summary>
	/// <param name="watchFd">门句柄</param>
	public abstract void Rm(WatchFdAbstract watchFd);
}

/// <summary>
/// 通过 <see cref="Task"/> 运行的看门狗
/// </summary>
public class TaskWatchDog : WatchDogPublic
{
	/// <summary>
	/// 检查任务
	/// </summary>
	private readonly Task _runTask;
	/// <summary>
	/// 创建看门狗
	/// </summary>
	/// <param name="period">检查间隔</param>
	public TaskWatchDog(int period) : base(period)
	{
		_runTask = new Task(run);
	}
	/// <summary>
	/// 启动看门狗
	/// </summary>
	public void Start()
	{
		_runTask.Start();
	}
}
/// <summary>
/// 通过 <see cref="Thread"/> 运行的看门狗
/// </summary>
public class ThreadWatchDog : WatchDogPublic
{
	/// <summary>
	/// 检查任务
	/// </summary>
	private readonly Thread _runThread;
	/// <summary>
	/// 创建看门狗
	/// </summary>
	/// <param name="period">检查间隔</param>
	public ThreadWatchDog(int period) : base(period)
	{
		_runThread = new Thread(run);
	}

	/// <summary>
	/// 启动看门狗
	/// </summary>
	public void Start()
	{
		_runThread.Start();
	}
}

/// <summary>
/// 具有看门狗公共实现的中间类
/// </summary>
public abstract class WatchDogPublic : WatchDogAbstract
{

	private readonly object _lock = new object(); 
	private List<WatchFdAbstract> _list = new List<WatchFdAbstract>();
	private readonly int _period;

	/// <summary>
	/// 具有看门狗公共实现的中间类
	/// </summary>
	/// <param name="period">检查间隔</param>
	protected WatchDogPublic(int period)
	{
		_period = period;
	}
	/// <summary>
	/// 注册门句柄
	/// </summary>
	/// <param name="watchFd">门句柄</param>
	public override void Reg(WatchFdAbstract watchFd)
	{
		lock (_lock)
		{
			_list.Add(watchFd);
		}
	}

	/// <summary>
	/// 检查主任务
	/// </summary>
	protected void run()
	{
		checked
		{
			while (true)
			{
				//async
				Task<WatchFdAbstract[]> task = new Task<WatchFdAbstract[]>(() =>
				{
					WatchFdAbstract[] watchFdAbstracts;
					lock (_lock)
					{
						watchFdAbstracts = _list.ToArray();
					}

					return watchFdAbstracts;
				});
				task.Start();
				//async end
				long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				WatchFdAbstract[] watchFds = task.Result;
				foreach (WatchFdAbstract watchFd in watchFds)
				{
					try
					{
						if (now > Interlocked.Read(ref watchFd.time))
						{
							watchFd.Rm();
							_list.Remove(watchFd);
						}
					}
					catch (Exception)
					{
						// ignored
					}
				}
				Thread.Sleep(_period);

			}
		}
	}
	
	/// <summary>
	/// 注销门句柄
	/// </summary>
	/// <param name="watchFd">门句柄</param>
	public override void Rm(WatchFdAbstract watchFd)
	{
		lock (_lock)
		{
			_list.Remove(watchFd);
		}
	}
}