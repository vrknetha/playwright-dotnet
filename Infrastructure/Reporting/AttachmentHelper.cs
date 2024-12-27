namespace ParkPlaceSample.Infrastructure.Reporting;

public static class AttachmentHelper
{
    public static string CreateAttachmentHtml(string filePath, string description)
    {
        var fileName = Path.GetFileName(filePath);
        var fileExtension = Path.GetExtension(filePath).ToLower();

        return fileExtension switch
        {
            ".webm" => CreateVideoAttachmentHtml(fileName),
            ".zip" => CreateTraceAttachmentHtml(fileName),
            _ => CreateGenericAttachmentHtml(fileName, description)
        };
    }

    private static string CreateVideoAttachmentHtml(string fileName)
    {
        return $@"<div class='card' style='margin: 10px 0;'>
            <div class='card-header'>
                <h5 class='card-title'>Test Execution Video</h5>
            </div>
            <div class='card-body'>
                <a href='Videos/{fileName}' class='btn btn-primary mb-3' download>
                    <i class='fa fa-download'></i> Download Video
                </a>
                <div style='position: relative; padding-bottom: 56.25%; height: 0; overflow: hidden;'>
                    <video style='position: absolute; top: 0; left: 0; width: 100%; height: 100%;' controls>
                        <source src='Videos/{fileName}' type='video/webm'>
                        Your browser does not support the video tag.
                    </video>
                </div>
            </div>
        </div>";
    }

    private static string CreateTraceAttachmentHtml(string fileName)
    {
        return $@"<div class='card' style='margin: 10px 0;'>
            <div class='card-header'>
                <h5 class='card-title'>Test Execution Trace</h5>
            </div>
            <div class='card-body'>
                <a href='Traces/{fileName}' class='btn btn-success mb-3' download>
                    <i class='fa fa-file-archive-o'></i> Download Trace
                </a>
                <div class='alert alert-info'>
                    <i class='fa fa-info-circle'></i> The trace file contains detailed information about the test execution, including network requests, console logs, and screenshots.
                </div>
            </div>
        </div>";
    }

    private static string CreateGenericAttachmentHtml(string fileName, string description)
    {
        return $@"<div class='card' style='margin: 10px 0;'>
            <div class='card-header'>
                <h5 class='card-title'>{description}</h5>
            </div>
            <div class='card-body'>
                <a href='{fileName}' class='btn btn-secondary' download>
                    <i class='fa fa-download'></i> Download File
                </a>
            </div>
        </div>";
    }
}