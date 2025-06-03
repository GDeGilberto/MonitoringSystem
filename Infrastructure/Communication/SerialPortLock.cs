namespace Infrastructure.Communication
{
    public static class SerialPortLock
    {
        public static readonly SemaphoreSlim Semaphore = new(1, 1);
    }
}