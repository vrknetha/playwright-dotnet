using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using NUnit.Framework;

namespace PlaywrightDemo.Infrastructure.Reporting;

public static class HtmlReporterConfig
{
    public static void ConfigureHtmlReporter(ExtentHtmlReporter htmlReporter)
    {
        htmlReporter.Config.DocumentTitle = "Playwright Test Execution Report";
        htmlReporter.Config.ReportName = "Test Automation Results";
        htmlReporter.Config.Theme = Theme.Standard;
        htmlReporter.Config.EnableTimeline = true;

        // Add test framework info
        htmlReporter.Config.JS = @"
            $(document).ready(function() {
                $('.test-content').each(function() {
                    var testName = $(this).find('.test-name').text();
                    var testCategory = $(this).find('.test-category').text();
                    var testDescription = $(this).find('.test-description').text();
                    
                    // Add test metadata
                    var metadataHtml = '<div class=""test-metadata"">';
                    if (testCategory) {
                        metadataHtml += '<span class=""badge badge-primary"">' + testCategory + '</span>';
                    }
                    if (testDescription) {
                        metadataHtml += '<div class=""test-description"">' + testDescription + '</div>';
                    }
                    metadataHtml += '</div>';
                    
                    $(this).find('.test-head').after(metadataHtml);
                });
            });
        ";

        htmlReporter.Config.CSS = @"
            /* Reset ExtentReports default styles */
            .test-content { border: none !important; }
            .test-content .test-steps { border: none !important; margin: 0 !important; padding: 0 !important; }
            .test-content .test-step { border: none !important; margin: 0 !important; padding: 0 !important; }
            .test-content .test-step td { border: none !important; padding: 4px 8px !important; }
            .test-content .test-step .step-details { padding: 0 !important; margin: 0 !important; }
            .test-content .test-step .step-details > div { margin: 4px 0 !important; }
            
            /* Test metadata styling */
            .test-metadata {
                padding: 10px;
                margin: 10px 0;
                background: #f8f9fa;
                border-radius: 4px;
            }
            
            .badge {
                display: inline-block;
                padding: 4px 8px;
                font-size: 12px;
                font-weight: 600;
                border-radius: 4px;
                margin-right: 8px;
            }
            
            .badge-primary {
                background: #0d6efd;
                color: white;
            }
            
            .test-description {
                margin-top: 8px;
                font-size: 14px;
                color: #6c757d;
            }
            
            /* Step status indicator styling */
            .test-content .test-step .status {
                display: inline-block !important;
                width: 20px !important;
                height: 20px !important;
                line-height: 20px !important;
                text-align: center !important;
                border-radius: 50% !important;
                margin-right: 8px !important;
                font-size: 12px !important;
                font-weight: bold !important;
            }
            .test-content .test-step .status.pass { background-color: #28a745 !important; color: white !important; }
            .test-content .test-step .status.fail { background-color: #dc3545 !important; color: white !important; }
            .test-content .test-step .status.warning { background-color: #ffc107 !important; color: #000 !important; }
            .test-content .test-step .status.info { background-color: #0d6efd !important; color: white !important; }
            .test-content .test-step .status.skip { background-color: #6c757d !important; color: white !important; }
            
            /* Step details styling */
            .test-content .test-step .step-details {
                display: flex !important;
                align-items: center !important;
                padding: 8px 12px !important;
                background-color: #f8f9fa !important;
                border-radius: 4px !important;
                margin: 4px 0 !important;
            }
            
            /* Step timestamp styling */
            .test-content .test-step .timestamp {
                color: #6c757d !important;
                font-size: 12px !important;
                margin-right: 12px !important;
                font-family: 'SFMono-Regular', Consolas, monospace !important;
            }
            
            /* Step details text styling */
            .test-content .test-step .step-details .details {
                flex: 1 !important;
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Arial, sans-serif !important;
                line-height: 1.5 !important;
            }
            
            /* Log message styling */
            .log-message {
                padding: 8px 12px;
                margin: 4px 0;
                border-radius: 4px;
                font-family: 'SFMono-Regular', Consolas, monospace;
                line-height: 1.5;
                background-color: #f8f9fa;
                border-left: 4px solid #0d6efd;
            }
            .log-message.warning {
                background-color: #fff3cd;
                border-left-color: #ffc107;
            }
            .log-message.error {
                background-color: #f8d7da;
                border-left-color: #dc3545;
            }
            
            /* Error details styling */
            .error-details {
                margin: 8px 0 0 12px;
                padding: 8px;
                background: #fff3f3;
                border-radius: 4px;
            }
            .error-message { margin-bottom: 8px; }
            .stack-trace { margin-top: 8px; }
            .stack-trace pre {
                margin: 4px 0;
                padding: 8px;
                font-size: 12px;
                background: #f8f9fa;
                border-radius: 4px;
                overflow-x: auto;
                font-family: 'SFMono-Regular', Consolas, monospace;
            }
            
            /* Card styling */
            .card {
                margin: 10px 0;
                border: 1px solid #dee2e6;
                border-radius: 4px;
                background: #fff;
                box-shadow: 0 1px 3px rgba(0,0,0,0.12);
            }
            .card-header {
                padding: 10px 15px;
                background-color: #f8f9fa;
                border-bottom: 1px solid #dee2e6;
                font-weight: 600;
            }
            .card-body { padding: 15px; }
            
            /* Button styling */
            .btn {
                display: inline-block;
                padding: 6px 12px;
                margin-bottom: 15px;
                font-size: 14px;
                font-weight: 500;
                text-align: center;
                text-decoration: none;
                border-radius: 4px;
                cursor: pointer;
                border: none;
                transition: background-color 0.2s;
            }
            .btn:hover { opacity: 0.9; }
            .btn-primary { background-color: #0d6efd; color: white !important; }
            .btn-success { background-color: #28a745; color: white !important; }
            .btn-secondary { background-color: #6c757d; color: white !important; }
            
            /* Alert styling */
            .alert {
                padding: 12px;
                margin: 8px 0;
                border-radius: 4px;
                border: 1px solid transparent;
            }
            .alert-info {
                background-color: #cff4fc;
                border-color: #b6effb;
                color: #055160;
            }
            .alert-warning {
                background-color: #fff3cd;
                border-color: #ffecb5;
                color: #664d03;
            }
            .alert-danger {
                background-color: #f8d7da;
                border-color: #f5c2c7;
                color: #842029;
            }
            
            /* Dashboard styling */
            .dashboard-view { padding: 20px; }
            .test-stats { margin-bottom: 30px; }
            .environment-info { 
                background-color: #f8f9fa; 
                padding: 15px; 
                border-radius: 4px; 
                margin-bottom: 20px;
                box-shadow: 0 1px 3px rgba(0,0,0,0.12);
            }
            .category-stats { 
                display: grid;
                grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
                gap: 20px; 
                margin-bottom: 30px; 
            }
            .category-item { 
                padding: 15px; 
                border-radius: 4px; 
                background-color: #fff; 
                box-shadow: 0 1px 3px rgba(0,0,0,0.12);
            }
            .timing-stats { margin-top: 20px; }
            .timing-item { 
                background: #fff;
                padding: 10px;
                border-radius: 4px;
                margin-bottom: 10px;
                box-shadow: 0 1px 3px rgba(0,0,0,0.12);
            }
            .test-analysis { margin-top: 30px; }
            .failure-pattern { 
                background-color: #fff3cd; 
                padding: 15px; 
                border-radius: 4px; 
                margin-bottom: 15px;
                border: 1px solid #ffecb5;
            }
            
            /* Timeline styling */
            .timeline-item-container { border: none !important; }
            .timeline-item { 
                margin: 4px 0 !important; 
                padding: 8px !important; 
                border-radius: 4px !important;
                background: #fff !important;
                box-shadow: 0 1px 3px rgba(0,0,0,0.12) !important;
            }
            
            /* Test artifacts styling */
            .artifact-card {
                background: #fff;
                border-radius: 4px;
                margin: 10px 0;
                box-shadow: 0 1px 3px rgba(0,0,0,0.12);
            }
            .artifact-header {
                padding: 10px;
                background: #f8f9fa;
                border-bottom: 1px solid #dee2e6;
                display: flex;
                align-items: center;
                gap: 8px;
            }
            .artifact-content {
                padding: 15px;
            }
            .artifact-preview {
                margin-bottom: 10px;
            }
            .artifact-preview img,
            .artifact-preview video {
                max-width: 100%;
                border-radius: 4px;
            }
            .artifact-details {
                display: flex;
                align-items: center;
                gap: 10px;
                font-size: 14px;
            }
            .artifact-download {
                color: #0d6efd;
                text-decoration: none;
            }
            .artifact-download:hover {
                text-decoration: underline;
            }
        ";
    }
}