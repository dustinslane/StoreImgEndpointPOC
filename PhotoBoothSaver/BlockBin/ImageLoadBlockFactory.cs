using System.Threading.Tasks.Dataflow;
using SixLabors.ImageSharp;

namespace PhotoBoothSaver.BlockBin;

public static class BlockFactory
{
    public static TransformBlock<FormFileDto, ImageFileDto> CreateImageLoadBlock(int threads, int capacity)
    {
        return new (dto =>
        {
            var form = dto.File;
                           
            using var memStream = new MemoryStream(form);
                           
            memStream.Position = 0;
                                       
            byte[] contents = memStream.ToArray();
                           
            var imageDto = new ImageFileDto
            {
                Content = contents,
                SavePath = dto.SavePath
            };
            return imageDto;
        }, new ExecutionDataflowBlockOptions()
        {
            BoundedCapacity = capacity,
            MaxDegreeOfParallelism = threads
        });
    }
}