# BlogMonitor

![License](https://img.shields.io/github/license/Maggiplant/BlogMonitor)

Program that monitors web pages, for example blogs, for changes

## Usage
 BlogMonitor [options] <url>

Arguments:
  <url>    URL to be monitored. The URL must be entered without specifying the scheme e.g. "example.com", NOT "https://example.com"
           or "http://example.com"

Options:
  -m, --mail             Turns on mailer function. The program will prompt you for your credentials.
  -d, --delay <delay>    Set a custom interval between checks in seconds [default: 900]
  --no-https             Use http instead of https when connecting to the specified URL
  --debug                Prints debug information to the console
  --version              Show version information
  -?, -h, --help         Show help and usage information
 
 Note that this program requires the .NET Framework to be installed in order for it to run. Specifically, this program targets .NET Core 3.1

## How it works
This program downloads the complete HTML of the web page that was given as an argument. It then uses HTML Agility Pack to select the \<p\> tags from the HtmlDocument and saves their contents to a string. This string is compared with the string previously downloaded, if the old and the new string differ, the program reports this and sends an email if the --mail flag is used.
