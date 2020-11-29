using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.CommandLine;
using System.CommandLine.Invocation;
using HtmlAgilityPack;

namespace BlogChecker
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Argument<string>("url", "URL to be monitored"),
                new Option(new[] {"--mail", "-m"}, "Turns on mailer function. The program will prompt you for your credentials."),
                new Option<int>(new[] {"--delay", "-d"}, getDefaultValue: () => 900, "Set a custom interval between checks in seconds, default value is 900"),
                new Option("--debug", "Prints debug information to the console")
            };

            rootCommand.Description = "Program that monitors web pages for changes";

            rootCommand.Handler = CommandHandler.Create<string, bool, int, bool>(HandleCommand);

            return rootCommand.Invoke(args);
        }

        private static void HandleCommand(string url, bool mail, int delay, bool debug)
        {
            // Declare global variables
            string emaillogin = null;
            string emailpassword = null;
            string recipientaddress = null;
            string output;
            string newstring;
            string oldstring = "";
            string urlplusscheme = "https://" + url; // Prepend scheme to url

            if (mail)
            {
                Console.WriteLine("Enter your email address: ");
                emaillogin = Console.ReadLine();

                Console.WriteLine("Enter your password: ");
                emailpassword = Console.ReadLine();

                Console.WriteLine("Enter email to be notified: ");
                recipientaddress = Console.ReadLine();
            }

            WebClient webClient = new WebClient();
            while (true)
            {
                // Get webpage and save to string
                string download = webClient.DownloadString(urlplusscheme);

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
                        Mailer(emaillogin, emailpassword, recipientaddress, url);
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

        private static void Mailer(string email, string pass, string recipient, string url)
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

            // Set up smtp client
            SmtpClient client = new SmtpClient
            {
                Host = "smtp.mail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = credentials
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