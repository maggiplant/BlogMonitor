using HtmlAgilityPack;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace BlogChecker
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Argument<string>("url", "URL to be monitored. The URL must be entered without specifying the scheme e.g. \"example.com\", NOT \"https://example.com\" or \"http://example.com\""),
                new Option(new[] {"--mail", "-m"}, "Turns on mailer function. The program will prompt you for your credentials."),
                new Option<int>(new[] {"--delay", "-d"}, getDefaultValue: () => 900, "Set a custom interval between checks in seconds"),
                new Option("--no-https", "Use http instead of https when connecting to the specified URL"),
                new Option("--debug", "Prints debug information to the console")
            };

            rootCommand.Description = "Program that monitors web pages for changes";

            rootCommand.Handler = CommandHandler.Create<string, bool, int, bool, bool>(HandleCommand);

            return rootCommand.Invoke(args);
        }

        private static void HandleCommand(string url, bool mail, int delay, bool http, bool debug)
        {
            // Declare global variables
            string emaillogin = null;
            string emailpassword = null;
            string recipientaddress = null;
#pragma warning disable IDE0059 // Visual studio will nag about download being unnecessarily assigned to without this.
            string download = null;
#pragma warning restore IDE0059 // download will only NOT be assigned to in case of a network error when trying to reach the URL of the page to be checked, in which case the program will exit anyway.
            int tries = 0;
            //TODO: check if assigning 0 to provider causes problems
            int provider = 0;
            string output;
            string newstring;
            string oldstring = "";
            string urlplusscheme;

            // Prepend scheme to url
            // When http is true, use http://, else use https://
            if (http)
            {
                urlplusscheme = "http://" + url;
            }
            else
            {
                urlplusscheme = "https://" + url;
            }

            if (mail)
            {
                //TODO: Add check so values that are NOT 1, 2 or 3 can not be entered
                Console.WriteLine("This program supports the following email providers: ");
                Console.WriteLine("1.\tGmail\n2.\tMail.com3.\t\nYahoo");
                Console.Write("Please select one by tying the corresponding number: ");
                provider = Console.Read();
                Console.WriteLine();

                Console.Write("Enter your email address: ");
                emaillogin = Console.ReadLine();
                Console.WriteLine();

                Console.Write("Enter your password: ");
                emailpassword = Console.ReadLine();
                Console.WriteLine();

                Console.Write("Enter email to be notified: ");
                recipientaddress = Console.ReadLine();

                Console.Clear();
                Console.WriteLine("Credentials entered.\nProgram Starting Now...\n");
            }

            WebClient webClient = new WebClient();
            while (true)
            {
                // Loop for retrying the download in case of an error
                while (true)
                {
                    try
                    {
                        // Get webpage and save to string
                        download = webClient.DownloadString(urlplusscheme);
                        // Break the "infinite" while loop if the download succeeds
                        break;
                    }
                    catch (Exception e)
                    {
                        if (tries < 3)
                        {
                            Console.WriteLine("Unable to reach URL. Retrying in 5 minutes.");
                            tries++;
                            Thread.Sleep(300000);
                        }
                        else
                        {
                            Console.WriteLine("The program was unable to reach the entered URL. The error message was: " + e.Message);
                            return;
                        }
                    }
                }

                // Load the page into an HTML Agility Pack HtmlDocument
                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(download);

                // Select all <p> tags in the HtmlDocument
                var allPTags = html.DocumentNode.SelectNodes("//p");

                // Initialize a stringwriter so the nodes can be written to a string
                StringWriter writer = new StringWriter();

                // Loop through loaded nodes and write them to the writer
                foreach (HtmlNode node in allPTags)
                {
                    node.WriteContentTo(writer);
                }

                // Use the stringwriter to write to string
                output = writer.ToString();

                // Compare requested webpage to old webpage
                newstring = output;
                if (oldstring != "" && !newstring.Equals(oldstring))
                {
                    Console.WriteLine("Change detected at " + DateTime.Now);
                    // Call Mailer method with emaillogin and emailpassword
                    if (mail)
                    {
                        Mailer(emaillogin, emailpassword, recipientaddress, url, provider);
                    }
                    // Print the contents of newstring when --debug flag is on
                    if (debug)
                    {
                        Console.WriteLine("debug: " + newstring);
                    }
                    Console.WriteLine("Notification sent!");
                }

                oldstring = newstring;
                Delayer(delay);
            }
        }

        private static void Delayer(int delay)
        {
            Thread.Sleep(delay * 1000); // Multiply delay by 1000 because it was entered in seconds
        }

        private static void Mailer(string email, string pass, string recipient, string url, int provider)
        {
            // Set up credentials beforehand
            NetworkCredential credentials = new NetworkCredential(email, pass);

            // Create message
            MailMessage message = new MailMessage
            {
                From = new MailAddress(email),
                Priority = MailPriority.High,
                Subject = "New blog post detected",
                Body = "This email was sent to inform you of a new blog post on " + url + "\n\nHave a nice day!"
            };
            message.To.Add(recipient);

            // Declare SmtpClient outside of switch/case, set the settings inside of it
            SmtpClient client = new SmtpClient();
            switch (provider)
            {
                case 1:
                    {
                        client.Host = "smtp.gmail.com";
                        client.Port = 587;
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = credentials;

                        break;
                    }
                case 2:
                    {
                        client.Host = "smtp.mail.com";
                        client.Port = 587;
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = credentials;

                        break;
                    }
                case 3:
                    {
                        client.Host = "smtp.mail.yahoo.com";
                        client.Port = 587;
                        client.EnableSsl = true;
                        client.UseDefaultCredentials = false;
                        client.Credentials = credentials;

                        break;
                    }
            };

            // Put the code to send the email inside try-catch since email providers like to give errors instead of sending mail
            try
            {
                // Actually send the message
                client.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong while sending an email. The error message was: " + e.Message);
            }
        }
    }
}