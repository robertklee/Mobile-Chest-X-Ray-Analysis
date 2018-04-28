##################################################################
#                       TensorFlow Restore                       
##################################################################
#
# This script ensures the tensorflow-android AAR file is present
# in the Jars directory. It should be run before the actual build
# of the TensorFlowAndroid project begins.
#
# This is the POWERSHELL version of this script for use when
# running builds on Windows.
#
# There is an equivalent bash script (prebuild) that gets used
# when running builds on a Mac.
#
##################################################################

Write-Output ""
Write-Output "================================================================="
Write-Output "TensorFlow Restore"
Write-Output "================================================================="
Write-Output ""

# From https://blogs.msdn.microsoft.com/jasonn/2008/06/13/downloading-files-from-the-internet-in-powershell-with-progress/
function DownloadFile($url, $targetFile, $timeoutMillis)
{
    Try
    {
        # Prepare and send request
        $uri = New-Object "System.Uri" "$url"
        $request = [System.Net.HttpWebRequest]::Create($uri)
        $request.set_Timeout($timeoutMillis)
        $request.set_ReadWriteTimeout($timeoutMillis)
        $response = $request.GetResponse()
        $totalLength = [System.Math]::Floor($response.get_ContentLength()/1024)

        # Prepare streams
        $responseStream = $response.GetResponseStream()
        $targetStream = New-Object -TypeName System.IO.FileStream -ArgumentList $targetFile, Create

        # Prepare buffer
        $buffer = new-object byte[] 10KB
        $count = $responseStream.Read($buffer,0,$buffer.length)
        $downloadedBytes = $count

        # Read through response and write to targetFile
        while ($count -gt 0)
        {
            $targetStream.Write($buffer, 0, $count)
            $count = $responseStream.Read($buffer,0,$buffer.length)
            $downloadedBytes = $downloadedBytes + $count
            Write-Progress -activity "Downloading file '$($url.split('/') | Select -Last 1)'" -status "Downloaded ($([System.Math]::Floor($downloadedBytes/1024))K of $($totalLength)K): " -PercentComplete ((([System.Math]::Floor($downloadedBytes/1024)) / $totalLength)  * 100)
        }

        Write-Progress -activity "Finished downloading file '$($url.split('/') | Select -Last 1)'"
    }
    Catch [Exception]
    {
        Write-Output $_.Exception|format-list -force
        Write-Output ""
        $error = 1
    }
    Finally
    {
        # Clean up resources
        $targetStream.Flush()
        $targetStream.Close()
        $targetStream.Dispose()
        $responseStream.Dispose()

        # Check if an error occurred
        if ($error -ne 0)
        {
            # If the file partially downloaded, delete it
            if ([System.IO.File]::Exists($targetFile))
            {
                Remove-Item -Path $targetFile
            }
            Write-Output "`nDownload failed.`n"
            exit 1
        }
    }
}

# Constants
$TFVersion = "1.5.0"
$TFPath = "$PSScriptRoot\Jars\tensorflow-android-$TFVersion.aar"
$TFUrl = "http://central.maven.org/maven2/org/tensorflow/tensorflow-android/$TFVersion/tensorflow-android-$TFVersion.aar"
$TIMEOUT = 3600000 # 1 hour (in ms)

# Check if the AAR already exists
Write-Output "---------- Checking for existing tensorflow-android AAR`n"
Write-Output ""

if ([System.IO.File]::Exists($TFPath))
{
    Write-Output "Existing AAR found!`n"
    Write-Output ""
    exit
}
else
{
    Write-Output "AAR not found`n"
}

# Attempt to download the AAR
Write-Output "---------- Fetching tensorflow-android AAR (version $TFVersion)`n"

DownloadFile $TFUrl $TFPath $TIMEOUT

Write-Output "`nDownload Succeeded!`n"
