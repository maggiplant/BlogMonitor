# BlogMonitor
Program that monitors web pages for changes

## Usage
 BlogMonitor [options] <url>

Arguments:
  <url>    URL to be monitored

Options:
  -m, --mail             Turns on mailer function. The program will prompt you for your credentials.
  -d, --delay <delay>    Set a custom interval between checks in seconds, default value is 900 [default: 900]
  --debug                Prints debug information to the console
  --version              Show version information
  -?, -h, --help         Show help and usage information

## How it works
This program downloads the complete HTML of the web page that was given as an argument. It then uses HTML Agility Pack to select the <p> tags from the HtmlDocument and saves their contents to a string. This string is compared with the string previously downloaded, if the old and the new string differ, the program reports this and sends an email if the --mail flag is used.
