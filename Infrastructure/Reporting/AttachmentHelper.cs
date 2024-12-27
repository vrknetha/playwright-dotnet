using System.Web;

namespace ParkPlaceSample.Infrastructure.Reporting;

public static class AttachmentHelper
{
    public static string CreateVideoAttachment(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var encodedPath = HttpUtility.HtmlEncode(filePath).Replace('\\', '/');

        return $@"
            <div class='artifact-card'>
                <div class='artifact-header'>
                    <span class='artifact-icon'>🎥</span>
                    <h3>Test Recording</h3>
                </div>
                <div class='artifact-content'>
                    <div class='video-container'>
                        <video controls preload='metadata'>
                            <source src='{encodedPath}' type='video/webm'>
                            Your browser does not support the video tag.
                        </video>
                    </div>
                    <div class='artifact-details'>
                        <a href='{encodedPath}' download class='download-link'>
                            <span class='download-icon'>⬇️</span>
                            Download Video
                        </a>
                        <span class='file-name'>{fileName}</span>
                    </div>
                </div>
            </div>";
    }

    public static string CreateTraceAttachment(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var encodedPath = HttpUtility.HtmlEncode(filePath).Replace('\\', '/');

        return $@"
            <div class='artifact-card'>
                <div class='artifact-header'>
                    <span class='artifact-icon'>📊</span>
                    <h3>Test Trace</h3>
                </div>
                <div class='artifact-content'>
                    <div class='trace-viewer'>
                        <div class='trace-instructions'>
                            <p>To view the test trace:</p>
                            <ol>
                                <li>Download the trace file</li>
                                <li>Visit <a href='https://trace.playwright.dev' target='_blank'>Playwright Trace Viewer</a></li>
                                <li>Upload the downloaded trace file</li>
                            </ol>
                        </div>
                        <div class='artifact-details'>
                            <a href='{encodedPath}' download class='download-link'>
                                <span class='download-icon'>⬇️</span>
                                Download Trace
                            </a>
                            <span class='file-name'>{fileName}</span>
                        </div>
                    </div>
                </div>
            </div>";
    }

    public static string CreateLogAttachment(string filePath, string logContent)
    {
        var fileName = Path.GetFileName(filePath);
        var encodedPath = HttpUtility.HtmlEncode(filePath).Replace('\\', '/');
        var encodedContent = HttpUtility.HtmlEncode(logContent);

        return $@"
            <div class='artifact-card'>
                <div class='artifact-header'>
                    <span class='artifact-icon'>📝</span>
                    <h3>Test Log</h3>
                </div>
                <div class='artifact-content'>
                    <div class='log-viewer'>
                        <pre class='log-content'>{encodedContent}</pre>
                        <div class='artifact-details'>
                            <a href='{encodedPath}' download class='download-link'>
                                <span class='download-icon'>⬇️</span>
                                Download Log
                            </a>
                            <span class='file-name'>{fileName}</span>
                        </div>
                    </div>
                </div>
            </div>";
    }

    public static string GetReportStyles()
    {
        return @"
            <style>
                .test-report {
                    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
                    max-width: 1200px;
                    margin: 0 auto;
                    padding: 20px;
                    background: #f8f9fa;
                }

                .test-header {
                    background: #fff;
                    border-radius: 8px;
                    padding: 20px;
                    margin-bottom: 20px;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                }

                .test-title {
                    font-size: 24px;
                    color: #2c3e50;
                    margin: 0 0 10px 0;
                }

                .test-info {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
                    gap: 15px;
                    margin-top: 15px;
                }

                .info-item {
                    display: flex;
                    flex-direction: column;
                }

                .info-label {
                    font-size: 12px;
                    color: #6c757d;
                    text-transform: uppercase;
                    letter-spacing: 0.5px;
                }

                .info-value {
                    font-size: 14px;
                    color: #2c3e50;
                    margin-top: 4px;
                }

                .test-status {
                    display: inline-block;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-weight: 500;
                    font-size: 14px;
                }

                .status-passed {
                    background: #d4edda;
                    color: #155724;
                }

                .status-failed {
                    background: #f8d7da;
                    color: #721c24;
                }

                .artifacts-section {
                    margin-top: 30px;
                }

                .artifacts-grid {
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
                    gap: 20px;
                    margin-top: 20px;
                }

                .artifact-card {
                    background: #fff;
                    border-radius: 8px;
                    overflow: hidden;
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                }

                .artifact-header {
                    background: #f8f9fa;
                    padding: 15px;
                    border-bottom: 1px solid #e9ecef;
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }

                .artifact-header h3 {
                    margin: 0;
                    font-size: 16px;
                    color: #2c3e50;
                }

                .artifact-icon {
                    font-size: 20px;
                }

                .artifact-content {
                    padding: 15px;
                }

                .video-container {
                    position: relative;
                    width: 100%;
                    padding-top: 56.25%; /* 16:9 Aspect Ratio */
                    margin-bottom: 10px;
                }

                .video-container video {
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    border-radius: 4px;
                    background: #000;
                }

                .artifact-details {
                    display: flex;
                    align-items: center;
                    gap: 15px;
                    margin-top: 10px;
                }

                .download-link {
                    display: flex;
                    align-items: center;
                    gap: 5px;
                    text-decoration: none;
                    color: #0d6efd;
                    font-size: 14px;
                }

                .download-link:hover {
                    text-decoration: underline;
                }

                .file-name {
                    font-size: 12px;
                    color: #6c757d;
                }

                .log-viewer {
                    background: #f8f9fa;
                    border-radius: 4px;
                    padding: 15px;
                }

                .log-content {
                    max-height: 300px;
                    overflow-y: auto;
                    font-family: 'Courier New', Courier, monospace;
                    font-size: 12px;
                    line-height: 1.5;
                    margin: 0;
                    padding: 10px;
                    background: #fff;
                    border-radius: 4px;
                    border: 1px solid #e9ecef;
                }

                .trace-instructions {
                    background: #e3f2fd;
                    border-radius: 4px;
                    padding: 15px;
                    margin-bottom: 15px;
                }

                .trace-instructions p {
                    margin: 0 0 10px 0;
                    font-weight: 500;
                }

                .trace-instructions ol {
                    margin: 0;
                    padding-left: 20px;
                }

                .trace-instructions li {
                    margin: 5px 0;
                    font-size: 14px;
                }

                .error-section {
                    margin-top: 20px;
                    background: #fff;
                    border-radius: 8px;
                    padding: 20px;
                    border: 1px solid #f8d7da;
                }

                .error-title {
                    color: #721c24;
                    margin: 0 0 15px 0;
                    font-size: 18px;
                }

                .error-message {
                    background: #f8d7da;
                    color: #721c24;
                    padding: 15px;
                    border-radius: 4px;
                    margin-bottom: 15px;
                }

                .stack-trace {
                    background: #f8f9fa;
                    padding: 15px;
                    border-radius: 4px;
                    font-family: 'Courier New', Courier, monospace;
                    font-size: 12px;
                    line-height: 1.5;
                    overflow-x: auto;
                    white-space: pre-wrap;
                }
            </style>";
    }
}