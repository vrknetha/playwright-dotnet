using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;

namespace ParkPlaceSample.Infrastructure.Reporting;

public static class HtmlReporterConfig
{
    public static void ConfigureHtmlReporter(ExtentHtmlReporter htmlReporter)
    {
        htmlReporter.Config.DocumentTitle = "Playwright Test Execution Report";
        htmlReporter.Config.ReportName = "Test Automation Results";
        htmlReporter.Config.Theme = Theme.Standard;
        htmlReporter.Config.EnableTimeline = true;

        htmlReporter.Config.CSS = @"
            /* Reset ExtentReports default styles */
            .test-content { border: none !important; }
            .test-content .test-steps { border: none !important; margin: 0 !important; padding: 0 !important; }
            .test-content .test-step { border: none !important; margin: 0 !important; padding: 0 !important; }
            .test-content .test-step td { border: none !important; padding: 4px 8px !important; }
            .test-content .test-step .step-details { padding: 0 !important; margin: 0 !important; }
            .test-content .test-step .step-details > div { margin: 4px 0 !important; }
            
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
                font-family: Consolas, monospace !important;
            }
            
            /* Step details text styling */
            .test-content .test-step .step-details .details {
                flex: 1 !important;
                font-family: 'Segoe UI', Arial, sans-serif !important;
                line-height: 1.5 !important;
            }
            
            /* Log message styling */
            .log-message {
                padding: 8px 12px;
                margin: 4px 0;
                border-radius: 4px;
                font-family: Consolas, monospace;
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
            }
            
            /* Card styling */
            .card {
                margin: 10px 0;
                border: 1px solid #dee2e6;
                border-radius: 4px;
                background: #fff;
            }
            .card-header {
                padding: 10px 15px;
                background-color: #f8f9fa;
                border-bottom: 1px solid #dee2e6;
            }
            .card-body { padding: 15px; }
            
            /* Button styling */
            .btn {
                display: inline-block;
                padding: 6px 12px;
                margin-bottom: 15px;
                font-size: 14px;
                font-weight: 400;
                text-align: center;
                text-decoration: none;
                border-radius: 4px;
                cursor: pointer;
            }
            .btn-primary { background-color: #0d6efd; color: white !important; }
            .btn-success { background-color: #28a745; color: white !important; }
            .btn-secondary { background-color: #6c757d; color: white !important; }
            
            /* Alert styling */
            .alert {
                padding: 12px;
                margin: 8px 0;
                border-radius: 4px;
            }
            .alert-info {
                background-color: #cff4fc;
                border: 1px solid #b6effb;
                color: #055160;
            }
            
            /* Dashboard styling */
            .dashboard-view { padding: 20px; }
            .test-stats { margin-bottom: 30px; }
            .environment-info { background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin-bottom: 20px; }
            .category-stats { display: flex; flex-wrap: wrap; gap: 20px; margin-bottom: 30px; }
            .category-item { flex: 1; min-width: 200px; padding: 15px; border-radius: 4px; background-color: #fff; box-shadow: 0 1px 3px rgba(0,0,0,0.12); }
            .timing-stats { margin-top: 20px; }
            .timing-item { margin-bottom: 10px; }
            .test-analysis { margin-top: 30px; }
            .failure-pattern { background-color: #fff3cd; padding: 10px; border-radius: 4px; margin-bottom: 10px; }
            
            /* Timeline styling */
            .timeline-item-container { border: none !important; }
            .timeline-item { margin: 4px 0 !important; padding: 8px !important; border-radius: 4px !important; }
        ";
    }
}