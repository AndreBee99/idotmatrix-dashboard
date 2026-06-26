# PowerShell script to send a real Windows toast notification for testing
# Usage: Run in PowerShell: .\send_test_notification.ps1 "Test Title" "This is a test notification message!"

param (
    [string]$Title = "Test Notification",
    [string]$Message = "Hello from PowerShell! This is a test toast message."
)

# Load WinRT assemblies
[Void][Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType=WindowsRuntime]
[Void][Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType=WindowsRuntime]

# Use a standard generic image-and-text template
$template = [Windows.UI.Notifications.ToastTemplateType]::ToastImageAndText02
$xml = [Windows.UI.Notifications.ToastNotificationManager]::GetTemplateContent($template)

# Populate Title and Message
$textNodes = @($xml.GetElementsByTagName("text"))
$textNodes[0].AppendChild($xml.CreateTextNode($Title)) | Out-Null
$textNodes[1].AppendChild($xml.CreateTextNode($Message)) | Out-Null

# Set up the toast notifier using PowerShell's AppId
$toast = [Windows.UI.Notifications.ToastNotification]::new($xml)
$notifier = [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier("Microsoft.Windows.PowerShell")
$notifier.Show($toast)

Write-Host "Sent toast notification: '$Title' - '$Message'" -ForegroundColor Green
