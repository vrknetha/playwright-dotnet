using System.Web;
using NUnit.Framework;
using AventStack.ExtentReports;

namespace ParkPlaceSample.Infrastructure.Reporting;

public static class AttachmentHelper
{
    public static string CreateVideoAttachment(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var encodedPath = HttpUtility.HtmlEncode($"../../{filePath}").Replace('\\', '/');

        // Add video to NUnit test context
        TestContext.AddTestAttachment(filePath, "Test Recording");

        return $@"
            <div class='artifact-card'>
                <div class='artifact-header'>
                    <span class='artifact-icon'>🎥</span>
                    <h3>Test Recording</h3>
                </div>
                <div class='artifact-content'>
                    <div class='video-container'>
                        <video controls preload='metadata' width='100%' height='auto'>
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
        var encodedPath = HttpUtility.HtmlEncode($"../../{filePath}").Replace('\\', '/');

        // Add trace to NUnit test context
        TestContext.AddTestAttachment(filePath, "Playwright Trace");

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
                                <li>Download the trace file using the link below</li>
                                <li>Run: <code>npx playwright show-trace {fileName}</code></li>
                            </ol>
                        </div>
                    </div>
                    <div class='artifact-details'>
                        <a href='{encodedPath}' download class='download-link'>
                            <span class='download-icon'>⬇️</span>
                            Download Trace
                        </a>
                        <span class='file-name'>{fileName}</span>
                    </div>
                </div>
            </div>";
    }

    public static string CreateLogAttachment(string filePath, string content)
    {
        var fileName = Path.GetFileName(filePath);
        var encodedPath = HttpUtility.HtmlEncode($"../../{filePath}").Replace('\\', '/');
        var encodedContent = HttpUtility.HtmlEncode(content);

        return $@"
            <div class='artifact-card'>
                <div class='artifact-header'>
                    <span class='artifact-icon'>📝</span>
                    <h3>Test Log</h3>
                </div>
                <div class='artifact-content'>
                    <div class='log-viewer'>
                        <pre class='log-content'>{encodedContent}</pre>
                    </div>
                    <div class='artifact-details'>
                        <a href='{encodedPath}' download class='download-link'>
                            <span class='download-icon'>⬇️</span>
                            Download Log
                        </a>
                        <span class='file-name'>{fileName}</span>
                    </div>
                </div>
            </div>";
    }

    public static string GetReportStyles()
    {
        return @"
            <style>
                .test-report { padding: 20px; }
                .test-header { margin-bottom: 30px; }
                .test-title { margin: 0 0 10px 0; }
                .test-status { 
                    display: inline-block;
                    padding: 5px 10px;
                    border-radius: 4px;
                    font-weight: bold;
                }
                .status-passed { background: #dff0d8; color: #3c763d; }
                .status-failed { background: #f2dede; color: #a94442; }
                .test-info { margin-top: 15px; }
                .info-item { margin: 5px 0; }
                .info-label { font-weight: bold; margin-right: 10px; }
                
                .artifacts-section { margin-top: 30px; }
                .artifacts-grid { 
                    display: grid;
                    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
                    gap: 20px;
                    margin-top: 20px;
                }
                
                .artifact-card {
                    border: 1px solid #ddd;
                    border-radius: 8px;
                    overflow: hidden;
                }
                
                .artifact-header {
                    background: #f8f9fa;
                    padding: 10px;
                    border-bottom: 1px solid #ddd;
                    display: flex;
                    align-items: center;
                    gap: 10px;
                }
                
                .artifact-content {
                    padding: 15px;
                }
                
                .video-container {
                    position: relative;
                    padding-bottom: 56.25%;
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
                
                .file-name {
                    color: #6c757d;
                    font-size: 14px;
                }
                
                .log-viewer {
                    max-height: 300px;
                    overflow-y: auto;
                    background: #f8f9fa;
                    border-radius: 4px;
                    padding: 10px;
                    margin-bottom: 10px;
                }
                
                .log-content {
                    margin: 0;
                    font-family: monospace;
                    font-size: 12px;
                    white-space: pre-wrap;
                }
                
                .error-section {
                    margin-top: 30px;
                    padding: 20px;
                    background: #f2dede;
                    border-radius: 4px;
                }
                
                .error-title {
                    color: #a94442;
                    margin-top: 0;
                }
                
                .error-message {
                    margin: 10px 0;
                }
                
                .stack-trace {
                    background: #fff;
                    padding: 10px;
                    border-radius: 4px;
                    margin-top: 10px;
                }
                
                .stack-trace pre {
                    margin: 0;
                    font-family: monospace;
                    font-size: 12px;
                    white-space: pre-wrap;
                }
            </style>";
    }
}