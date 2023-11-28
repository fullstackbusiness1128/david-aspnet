using Microsoft.Extensions.Logging;
using FunTokenz.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;

namespace FunTokenz.Services
{
    public class Comms : IComms
    {

        private readonly ILogger<Comms> _logger;

        public Comms(ILogger<Comms> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendPurchaseEmail(EmailPurchase model)
        {

            try
            {

                string senderAddress = "hi@funtokenz.com";
                string receiverAddress = model.EmailAddressTo;
                string subject = "Your Fun Token";
                string htmlBody = "";
                string textBody = "";

                using (var client = new AmazonSimpleEmailServiceV2Client(RegionEndpoint.EUCentral1))
                {
                    var sendRequest = new SendEmailRequest
                    {
                        FromEmailAddress = senderAddress,
                        Destination = new Destination
                        {
                            ToAddresses = new List<string> { receiverAddress }
                        },
                        Content = new EmailContent
                        {
                            Simple = new Message
                            {
                                Subject = new Content() { Charset = "UTF-8", Data = subject },
                                Body = new Body
                                {
                                    Html = new Content
                                    {
                                        Charset = "UTF-8",
                                        Data = htmlBody
                                    },
                                    Text = new Content
                                    {
                                        Charset = "UTF-8",
                                        Data = textBody
                                    }
                                }
                            }
                        },
                    };
                    try
                    {
                        var response = client.SendEmailAsync(sendRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Email Error: " + ex);
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.LogInformation("Sending email error: " + exc);
            }

            return true;
        }

        public async Task<bool> SendForgotPasswordEmail(EmailForgot model)
        {

            try
            {
                string senderAddress = "hi@funtokenz.com";
                string receiverAddress = model.EmailAddressTo;
                string subject = "Password Reset Email";
                string textBody = "";
                string htmlBody = @"<html><head></head>
                <body>
                  <h1>FunTokenz.com Password Reset Email</h1>
                  <p>You have requested to reset your password. Please click on the link below to reset your password</p>
                  <a href='" + model.Link + @"'>" + model.Link + @"</a>
                </body>
                </html>";

                using (var client = new AmazonSimpleEmailServiceV2Client(RegionEndpoint.EUCentral1))
                {
                    var sendRequest = new SendEmailRequest
                    {
                        FromEmailAddress = senderAddress,
                        Destination = new Destination
                        {
                            ToAddresses =
                            new List<string> { receiverAddress }
                        },
                        Content = new EmailContent
                        {
                            Simple = new Message
                            {
                                Subject = new Content() { Charset = "UTF-8", Data = subject },
                                Body = new Body
                                {
                                    Html = new Content
                                    {
                                        Charset = "UTF-8",
                                        Data = htmlBody
                                    },
                                    Text = new Content
                                    {
                                        Charset = "UTF-8",
                                        Data = textBody
                                    }
                                }
                            }
                        },
                    };
                    try
                    {
                        var response = client.SendEmailAsync(sendRequest);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Forgot Password Email Error: " + ex);
                    }
                }

            }
            catch (Exception exc)
            {
                _logger.LogInformation("Sending Forgot Password Email Error: " + exc);
            }

            return true;
        }

    }
}
