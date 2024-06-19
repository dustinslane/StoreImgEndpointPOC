using System.Collections.Concurrent;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace PhotoBoothSaver;

public class Processor : BackgroundService
{
    private ConcurrentQueue<ImageFileDto> _queue = new ();

    private BackgroundService _backgroundService;

    private Rectangle Crop = new ((1920-960)/2, 0, 960, 1080);
    public void Queue(ImageFileDto content)
    {
        _queue.Enqueue(content);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(async () =>
        {
            await ProcessQueue(stoppingToken);
        }, stoppingToken);
        
        return Task.FromResult(0);
    }
    
    public async Task ProcessQueue(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var item))
            {
                await ProcessQueueItem(item, stoppingToken);
            }
        } 
    }

    private async Task ProcessQueueItem(ImageFileDto item, CancellationToken stoppingToken)
    {
        try
        {
            using var img = Image.Load(new DecoderOptions(), new ReadOnlySpan<byte>(item.Content));
            
            img.Mutate(x => x.Crop(cropRectangle: Crop));
            
            StringBuilder sb = new StringBuilder();
            
            sb.Append($@"{item.SavePath}");
            
            Directory.CreateDirectory(sb.ToString());

            await img.SaveAsWebpAsync(sb.ToString(), stoppingToken);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}